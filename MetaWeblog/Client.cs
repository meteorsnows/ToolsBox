using System;
using System.Collections.Generic;
using System.Linq;
using MSDev.MetaWeblog.XmlRPC;

namespace MSDev.MetaWeblog
{
    public class Client
    {
        //http://xmlrpc.scripting.com/metaWeblogApi.html

        public string AppKey = "0123456789ABCDEF";
        public BlogConnectionInfo BlogConnectionInfo;

        public Client(BlogConnectionInfo connectionInfo)
        {
            this.BlogConnectionInfo = connectionInfo;
        }

        public List<PostInfo> GetRecentPosts(int numposts)
        {
            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);

            var method = new MethodCall("metaWeblog.getRecentPosts");
            method.Parameters.Add(this.BlogConnectionInfo.BlogID);
            method.Parameters.Add(this.BlogConnectionInfo.Username);
            method.Parameters.Add(this.BlogConnectionInfo.Password);
            method.Parameters.Add(numposts);

            service.Cookies = this.BlogConnectionInfo.Cookies;

            var response = service.Execute(method);

            var param = response.Parameters[0];
            var array = (XmlRPC.Array)param;

            var items = new List<PostInfo>();
            foreach (var value in array)
            {
                var struct_ = (Struct)value;

                var postinfo = new PostInfo
                {
                    Title = struct_.Get("title", StringValue.NullString).String,
                    DateCreated = struct_.Get<DateTimeValue>("dateCreated").Data,
                    Link = struct_.Get("link", StringValue.NullString).String,
                    PostID = struct_.Get("postid", StringValue.NullString).String,
                    UserID = struct_.Get("userid", StringValue.NullString).String,
                    CommentCount = struct_.Get<IntegerValue>("commentCount", 0).Integer,
                    PostStatus = struct_.Get("post_status", StringValue.NullString).String,
                    PermaLink = struct_.Get("permaLink", StringValue.NullString).String,
                    Description = struct_.Get("description", StringValue.NullString).String
                };

                items.Add(postinfo);
            }
            return items;
        }

        public MediaObjectInfo NewMediaObject(string name, string type, byte[] bits)
        {
            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);

            var input_struct_ = new Struct();
            input_struct_["name"] = new StringValue(name);
            input_struct_["type"] = new StringValue(type);
            input_struct_["bits"] = new Base64Data(bits);

            var method = new MethodCall("metaWeblog.newMediaObject");
            method.Parameters.Add(this.BlogConnectionInfo.BlogID);
            method.Parameters.Add(this.BlogConnectionInfo.Username);
            method.Parameters.Add(this.BlogConnectionInfo.Password);
            method.Parameters.Add(input_struct_);

            service.Cookies = this.BlogConnectionInfo.Cookies;

            var response = service.Execute(method);
            var param = response.Parameters[0];
            var struct_ = (Struct)param;

            var mediaobject = new MediaObjectInfo
            {
                URL = struct_.Get("url", StringValue.NullString).String
            };

            return mediaobject;
        }

        public PostInfo GetPost(string postid)
        {
            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);

            var method = new MethodCall("metaWeblog.getPost");
            method.Parameters.Add(postid); // notice this is the postid, not the blogid
            method.Parameters.Add(this.BlogConnectionInfo.Username);
            method.Parameters.Add(this.BlogConnectionInfo.Password);

            service.Cookies = this.BlogConnectionInfo.Cookies;

            var response = service.Execute(method);
            var param = response.Parameters[0];
            var struct_ = (Struct)param;

            var postinfo = new PostInfo
            {
                //item.Categories 
                PostID = struct_.Get<IntegerValue>("postid").ToString(),
                Description = struct_.Get<StringValue>("description").String,
                //item.Tags
                Link = struct_.Get("link", StringValue.NullString).String,
                DateCreated = struct_.Get<DateTimeValue>("dateCreated").Data,
                PermaLink = struct_.Get("permaLink", StringValue.NullString).String,
                PostStatus = struct_.Get("post_status", StringValue.NullString).String,
                Title = struct_.Get<StringValue>("title").String,
                UserID = struct_.Get("userid", StringValue.NullString).String
            };

            var rawCats = struct_.Get<XmlRPC.Array>("categories");

            rawCats.ToList().ForEach(i =>
            {
                if (i is StringValue)
                {
                    var cat = (i as StringValue).String;

                    if (cat != "" && !postinfo.Categories.Contains(cat))
                        postinfo.Categories.Add(cat);

                }
            });

            return postinfo;
        }

        public string NewPost(PostInfo pi, IList<string> categories, bool publish = true)
        {
            return NewPost(pi.Title, pi.Description, categories, publish, pi.DateCreated);
        }

        public string NewPost(string title, string description, IList<string> categories, bool publish, DateTime? date_created)
        {
            XmlRPC.Array cats = null;

            if (categories == null)
            {
                cats = new XmlRPC.Array(0);
            }
            else
            {
                cats = new XmlRPC.Array(categories.Count);

                var ss = new List<Value>();

                categories.Select(c => new StringValue(c)).ToList().ForEach(i => ss.Add(i));

                cats.AddRange(ss);
            }

            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);

            var struct_ = new Struct();
            struct_["title"] = new StringValue(title);
            struct_["description"] = new StringValue(description);
            struct_["categories"] = cats;
            if (date_created.HasValue)
            {
                struct_["dateCreated"] = new DateTimeValue(date_created.Value);
                struct_["date_created_gmt"] = new DateTimeValue(date_created.Value.ToUniversalTime());

            }
            var method = new MethodCall("metaWeblog.newPost");
            method.Parameters.Add(this.BlogConnectionInfo.BlogID);
            method.Parameters.Add(this.BlogConnectionInfo.Username);
            method.Parameters.Add(this.BlogConnectionInfo.Password);
            method.Parameters.Add(struct_);
            method.Parameters.Add(publish);

            service.Cookies = this.BlogConnectionInfo.Cookies;

            var response = service.Execute(method);
            var param = response.Parameters[0];
            var postid = ((StringValue)param).String;

            return postid;
        }

        public bool DeletePost(string postid)
        {
            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);

            var method = new MethodCall("blogger.deletePost");
            method.Parameters.Add(AppKey);
            method.Parameters.Add(postid);
            method.Parameters.Add(this.BlogConnectionInfo.Username);
            method.Parameters.Add(this.BlogConnectionInfo.Password);
            method.Parameters.Add(true);

            service.Cookies = this.BlogConnectionInfo.Cookies;

            var response = service.Execute(method);

            var param = response.Parameters[0];
            var success = (BooleanValue)param;

            return success.Boolean;
        }

        public List<BlogInfo> GetUsersBlogs()
        {
            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);

            var method = new MethodCall("blogger.getUsersBlogs");
            method.Parameters.Add(this.AppKey);

            //if (!String.IsNullOrEmpty(this.BlogConnectionInfo.Username))
                method.Parameters.Add(this.BlogConnectionInfo.Username);

            //if (!String.IsNullOrEmpty(this.BlogConnectionInfo.Password))
                method.Parameters.Add(this.BlogConnectionInfo.Password);

            service.Cookies = this.BlogConnectionInfo.Cookies;

            var response = service.Execute(method);
            var list = (XmlRPC.Array)response.Parameters[0];

            var blogs = new List<BlogInfo>(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var struct_ = (Struct)list[i];

                var boginfo = new BlogInfo
                {
                    BlogID = struct_.Get("blogid", StringValue.NullString).String,
                    URL = struct_.Get("url", StringValue.NullString).String,
                    BlogName = struct_.Get("blogName", StringValue.NullString).String,
                    IsAdmin = struct_.Get<BooleanValue>("isAdmin", false).Boolean,
                    SiteName = struct_.Get("siteName", StringValue.NullString).String,
                    Capabilities = struct_.Get("capabilities", StringValue.NullString).String,
                    XmlRPCEndPoint = struct_.Get("xmlrpc", StringValue.NullString).String
                };

                blogs.Add(boginfo);
            }

            return blogs;
        }

        public bool EditPost(string postid, string title, string description, IList<string> categories, bool publish)
        {

            // Create an array to hold any categories
            var categories_ = new XmlRPC.Array(categories == null ? 0 : categories.Count);

            if (categories != null)
            {
                var sorted = categories.Distinct().ToList();

                sorted.Sort();

                var ss = new List<Value>();

                sorted.Select(c => new StringValue(c)).ToList().ForEach(i => ss.Add(i));

                categories_.AddRange(ss);
            }

            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);
            var struct_ = new Struct();

            struct_["title"] = new StringValue(title);
            struct_["description"] = new StringValue(description);
            struct_["categories"] = categories_;

            var method = new MethodCall("metaWeblog.editPost");
            method.Parameters.Add(postid);
            method.Parameters.Add(this.BlogConnectionInfo.Username);
            method.Parameters.Add(this.BlogConnectionInfo.Password);
            method.Parameters.Add(struct_);
            method.Parameters.Add(publish);

            service.Cookies = this.BlogConnectionInfo.Cookies;

            var response = service.Execute(method);
            var param = response.Parameters[0];
            var success = (BooleanValue)param;

            return success.Boolean;
        }

        public List<CategoryInfo> GetCategories()
        {
            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);

            var method = new MethodCall("metaWeblog.getCategories");
            method.Parameters.Add(this.BlogConnectionInfo.BlogID);
            method.Parameters.Add(this.BlogConnectionInfo.Username);
            method.Parameters.Add(this.BlogConnectionInfo.Password);
            service.Cookies = this.BlogConnectionInfo.Cookies;
            var response = service.Execute(method);

            var param = response.Parameters[0];
            var array = (XmlRPC.Array)param;

            var items = new List<CategoryInfo>();
            foreach (var value in array)
            {
                var struct_ = (Struct)value;

                var catinfo = new CategoryInfo
                {
                    Title = struct_.Get("title", StringValue.NullString).String,
                    Description = struct_.Get("description", StringValue.NullString).String,
                    HTMLURL = struct_.Get("htmlUrl", StringValue.NullString).String,
                    RSSURL = struct_.Get("rssUrl", StringValue.NullString).String,
                    CategoryID = struct_.Get("categoryid", StringValue.NullString).String
                };

                items.Add(catinfo);
            }
            return items;
        }

        public UserInfo GetUserInfo()
        {
            var service = new Service(this.BlogConnectionInfo.MetaWeblogURL);

            var method = new MethodCall("blogger.getUserInfo");
            method.Parameters.Add(this.AppKey);
            method.Parameters.Add(this.BlogConnectionInfo.Username);
            method.Parameters.Add(this.BlogConnectionInfo.Password);

            service.Cookies = this.BlogConnectionInfo.Cookies;

            var response = service.Execute(method);
            var param = response.Parameters[0];
            var struct_ = (Struct)param;
            var item = new UserInfo
            {
                UserID = struct_.Get("userid", StringValue.NullString).String,
                Nickname = struct_.Get("nickname", StringValue.NullString).String,
                FirstName = struct_.Get("firstname", StringValue.NullString).String,
                LastName = struct_.Get("lastname", StringValue.NullString).String,
                URL = struct_.Get("url", StringValue.NullString).String
            };

            return item;
        }
    }
}