﻿using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Data
{
    public class DataAccess : IDataAccess
    {
        private readonly string _connectionString;

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="connectionString"></param>
        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="config"></param>
        public DataAccess(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Photocontest");
        }

        /// <summary>
        /// get all the informations about a photo
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns>a single photo based on the id</returns>
        public Photo GetPhoto(int photoId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var getPhotoQuery = @"SELECT t18bp.id          as Id
                                     ,t18bp.url          as Url
                                     ,t18bp.votes        as Votes
                                     ,t18bp.total        as Total
                                     ,t18bp.average      as Average
                                     ,aspU.""UserName""    as UserName
                                     ,t18bp.thumbnailurl as ThumbnailUrl
                                     ,t18bp.uploadtimestamp as UploadTimestamp
                                     ,t18bp.title as Title
                                     ,t18bp.description as Description
                                     ,aspU.""Id"" as UserId
                                    FROM ""AspNetUsers"" aspU
                                           JOIN tsac18_bravo_photo t18bp on aspU.""Id"" = t18bp.upload_user_id
                                    WHERE t18bp.id = @id";
                return connection.QueryFirstOrDefault<Photo>(getPhotoQuery, new { id = photoId });
            }
        }

        /// <summary>
        /// Get a list of photos
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Photo> GetPhotos()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = @"SELECT t18bp.id as Id
                                      ,t18bp.url as Url
                                      ,t18bp.votes as Votes
                                      ,t18bp.total as Total
                                      ,t18bp.average as Average
                                      ,aspU.""UserName"" as UserName
                                      ,t18bp.thumbnailurl as ThumbnailUrl
                                FROM ""AspNetUsers"" aspU
                                JOIN tsac18_bravo_photo t18bp on aspU.""Id"" = t18bp.upload_user_id
                                order by t18bp.uploadtimestamp desc";
                return connection.Query<Photo>(query);
            }
        }

        /// <summary>
        /// Insert in the database a row with the information about a new photo
        /// </summary>
        /// <param name="photo"></param>
        public void AddPhoto(Photo photo)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var insertPhotoQuery = @"INSERT INTO tsac18_bravo_photo(url, votes, total, average, upload_user_id, title, description, uploadtimestamp) 
                                                    VALUES (@Url, @Votes, @Total, @Average, @UserName, @Title, @Description, @UploadTimestamp);";
                connection.Execute(insertPhotoQuery, photo);
            }
        }

        /// <summary> 
        /// return the informations about a vote if the user voted that photo
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Vote GetPhotoUser(int photoId, string userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var getPhotoUserQuery = @"SELECT t18bvu.id as Id
                                                ,user_id as UserId
                                                ,photo_id as PhotoId
                                                ,rating as Rating
                                          FROM tsac18_bravo_vote_user t18bvu 
                                                left join tsac18_bravo_photo t18bp 
                                                on t18bvu.photo_id = t18bp.id
                                           WHERE photo_id = @id AND user_id = @user";
                return connection.QueryFirstOrDefault<Vote>(getPhotoUserQuery, new { id = photoId, user = userId });
            }
        }

        /// <summary>
        /// return a list of photos ordered by average and votes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Photo> GetRanking()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var getRankingQuery = @"SELECT t18bp.id as Id
                                            ,t18bp.url as Url
                                            ,t18bp.votes as Votes
                                            ,t18bp.total as Total
                                            ,t18bp.average as Average
                                            ,aspU.""UserName"" as UserName 
                                            ,t18bp.thumbnailurl as ThumbnailUrl
                                            ,t18bp.uploadtimestamp as UploadTimestamp
                                            ,(100 * t18bp.average) + (10 * t18bp.votes) as Score
                                    FROM ""AspNetUsers"" aspU 
                                            JOIN tsac18_bravo_photo t18bp on aspU.""Id"" = t18bp.upload_user_id
                                    ORDER BY Score desc, UploadTimestamp desc
                                    LIMIT 6";
                return connection.Query<Photo>(getRankingQuery);
            }
        }

        /// <summary>
        /// update the database with the new vote
        /// </summary>
        /// <param name="vote"></param>
        /// <param name="photo"></param>
        public void AddVote(Vote vote, Photo photo)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var insertVoteQuery = @"INSERT INTO tsac18_bravo_vote_user(user_id, photo_id, rating) 
                                                        VALUES (@UserId, @PhotoId, @Rating)";

                    connection.Execute(insertVoteQuery, vote, transaction);

                    var updateVotesQuery = @"UPDATE tsac18_bravo_photo
                                            SET Votes = @Votes
                                            ,Total = @Total
                                            ,Average = @Average
                                             WHERE Id = @Id";

                    connection.Execute(updateVotesQuery, photo, transaction);

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// update the title and the description of a photo
        /// </summary>
        /// <param name="photo"></param>
        public void UpdatePhoto(Photo photo)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var updatePhotoQuery = @"UPDATE tsac18_bravo_photo
                                        SET title = @Title
                                          ,description = @Description
                                         WHERE Id = @Id";
                connection.Execute(updatePhotoQuery, photo);
            }
        }
    }
}
