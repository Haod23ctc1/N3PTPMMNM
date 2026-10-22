using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.EF;
using WebsiteNoiThat.Common;

namespace Models.DAO
{
   public class UserDao : IDisposable
    {
        DBNoiThat db = new DBNoiThat();

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
            GC.SuppressFinalize(this);
        }

        public int Insert(User acount)
        {
            db.Users.Add(acount);
            db.SaveChanges();
            return acount.UserId;
        }
        public User GetById(string username)
        {
            return db.Users.SingleOrDefault(n => n.Username == username);
        }

        /// <summary>
        /// Tìm user cho màn hình đăng nhập: cho phép gõ Tên đăng nhập HOẶC Email HOẶC Số điện thoại (issue #54)
        /// </summary>
        public User GetUserForLogin(string userInput)
        {
            userInput = (userInput ?? "").Trim();
            if (string.IsNullOrEmpty(userInput))
            {
                return null;
            }

            var user = db.Users.FirstOrDefault(x => x.Username == userInput);
            if (user == null)
            {
                user = db.Users.FirstOrDefault(x => x.Email == userInput);
            }
            if (user == null && System.Text.RegularExpressions.Regex.IsMatch(userInput, @"^\d{9,11}$"))
            {
                // SĐT trong DB lưu dạng int nên bỏ số 0 đầu (0901234567 -> 901234567)
                int phoneValue = int.Parse(userInput);
                user = db.Users.FirstOrDefault(x => x.Phone == phoneValue);
            }
            return user;
        }
        
        public int Login(string userName, string passWord, bool isLoginAdmin = false)
        {
            var result = GetUserForLogin(userName);
            if (result == null)
            {
                return 0;
            }
            else
            {
                if (isLoginAdmin == true)
                {
                    if ((result.GroupId  != CommonConstant.USER_GROUP && result.GroupId != CommonConstant.MEMBER_GROUP))
                    {
                        //if (result.Status == false)
                        //{
                        //    return -1;
                        //}

                        if (result.Password.Trim() == passWord)
                            {
                                return 1;
                            }
                            else
                            {
                                return -2;
                            }
                    }
                    else
                    {
                        return -3;
                    }
                }
                else
                {
                    if (result.Status == false)
                    {
                        return -1;
                    }
                    else
                    {
                        if (result.Password.Trim() == passWord)
                        {
                            return 1;
                        }
                        else
                        {
                            return -2;
                        }
                    }
                }
            }
        }
        public List<string> GetListCredentials(string userName)
        {
            var user = db.Users.Single(x => x.Username == userName);
            var data = (from a in db.Credentials
                        join b in db.UserGroups on a.UserGroupId equals b.GroupId
                        join c in db.Roles
                        on a.RoleId equals c.RoleId
                        where b.GroupId == user.GroupId
                        select new
                        {
                            RoleId = a.RoleId,
                            UserGroupId = a.UserGroupId
                        }).AsEnumerable().Select(x => new Credential()
                        {
                            RoleId = x.RoleId,
                            UserGroupId = x.UserGroupId
                        });
            return data.Select(x => x.RoleId).ToList();

        }
        public bool CheckUserName(string userName)
        {
            return db.Users.Count(x => x.Username== userName) > 0;
        }
        public bool CheckEmail(string email)
        {
            return db.Users.Count(x => x.Email == email) > 0;
        }

        //public int LoginAdmin(string userName, string passWord, bool isLoginAdmin = false)
        //{
        //    var models = db.Users.Where(n => n.GroupId == "ADMIN" || n.GroupId == "MOD").ToList();
        //    var result = models.FirstOrDefault(x => x.Username == userName &&  x.Password == passWord);
        //    if (result == null)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        if (isLoginAdmin == true)
        //        {
        //            if ((result.GroupId == CommonConstant.ADMIN_GROUP || result.GroupId == CommonConstant.MOD_GROUP))
        //            {
        //                if (result.Status == false)
        //                {
        //                    return -1;
        //                }
        //                else
        //                {
        //                    if (result.Password == passWord)
        //                    {

        //                        return 1;
        //                    }
        //                    else
        //                    {
        //                        return -2;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                return -3;
        //            }
        //        }
        //        else
        //        {
        //            if (result.Status == false)
        //            {
        //                return -1;
        //            }
        //            else
        //            {
        //                if (result.Password == passWord)
        //                {
        //                    return 1;
        //                }
        //                else
        //                {
        //                    return -2;
        //                }
        //            }
        //        }



        //    }
        //}

    }
}
