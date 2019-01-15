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

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataAccess(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Photocontest");
        }

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

        public void AddVote(Vote vote)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = @"insert into tsac18_bravo_vote_user(user_id, photo_id, rating) 
                                                        values (@UserId, @PhotoId, @Rating)";
                connection.Execute(query, vote);
            }
        }

        public void AddPhoto(Photo photo)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = @"INSERT INTO tsac18_bravo_photo(url, votes, total, average, upload_user) 
                                                    VALUES (@Url, @Votes, @Total, @Average, @UserName);";
                connection.Execute(query, photo);
            }
        }

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
                            ORDER BY Average desc";
                return connection.Query<Photo>(query);
            }
        }
    }
}
