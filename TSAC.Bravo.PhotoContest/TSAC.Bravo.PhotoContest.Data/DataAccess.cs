using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
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
        /// Get all the informations about a photo
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Photo GetPhoto(int Id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = @"SELECT t18bp.id as Id
                                    ,t18bp.url as Url
                                    ,t18bp.votes as Votes
                                    ,t18bp.total as Total
                                    ,t18bp.average as Average
                                    ,aspU.""UserName"" as UserName 
                            FROM ""AspNetUsers"" aspU 
                                    join tsac18_bravo_photo t18bp on aspU.""Id"" = t18bp.upload_user
                            WHERE t18bp.id = @id";
                return connection.QueryFirstOrDefault<Photo>(query, new { id = Id });
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
                                FROM ""AspNetUsers"" aspU
                                join tsac18_bravo_photo t18bp on aspU.""Id"" = t18bp.upload_user";
                return connection.Query<Photo>(query);
            }
        }

        /// <summary>
        /// Update the database with the number of votes, an algebric sum of all the votes and an average vote based on the result of a division
        /// </summary>
        /// <param name="photo"></param>
        public void AddVotePhoto(Photo photo)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = @"UPDATE tsac18_bravo_photo
                            SET Votes = @Votes
                            ,Total = @Total
                            ,Average = @Average
                             WHERE Id = @Id";
                connection.Execute(query, photo);
            }
        }

        /// <summary>
        /// Insert in the database a row the user id of the user that voted a photo, the photo id of the image and score given
        /// </summary>
        /// <param name="vote"></param>
        public void AddVote(Vote vote)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = @"insert into tsac18_bravo_vote_user(user_id, photo_id, rating) 
                                                        values (@UserId, @PhotoId, @Rating)";
                connection.Execute(query, vote);
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
                var query = @"INSERT INTO tsac18_bravo_photo(url, votes, total, average, upload_user) 
                                                    VALUES (@Url, @Votes, @Total, @Average, @UserName);";
                connection.Execute(query, photo);
            }
        }

        /// <summary>
        /// return the informations about a vote if the user voted that photo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Vote GetPhotoUser(int id, string userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = @"select 
                                    t18bvu.id as Id
                                    ,user_id as UserId
                                    ,photo_id as PhotoId
                                    ,rating as Rating
                          from tsac18_bravo_vote_user t18bvu 
                                left join tsac18_bravo_photo t18bp 
                                on t18bvu.photo_id = t18bp.id
                           where photo_id = @Id and user_id = @User";
                return connection.QueryFirstOrDefault<Vote>(query, new { Id = id, User = userId });
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
                var query = @"SELECT t18bp.id as Id
                                    ,t18bp.url as Url
                                    ,t18bp.votes as Votes
                                    ,t18bp.total as Total
                                    ,t18bp.average as Average
                                    ,aspU.""UserName"" as UserName 
                            FROM ""AspNetUsers"" aspU 
                                    join tsac18_bravo_photo t18bp on aspU.""Id"" = t18bp.upload_user
                            ORDER BY Average desc, Votes desc";
                return connection.Query<Photo>(query);
            }
        }
    }
}
