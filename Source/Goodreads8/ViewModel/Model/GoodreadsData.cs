using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using Goodreads8.ViewModel.Model;
using Windows.Data.Html;

namespace Goodreads8.ViewModel.Model
{
    class GoodreadsData
    {
        public static Profile ParseProfile(String input)
        {
            XDocument doc = XDocument.Parse(input);
            Profile profile = (from p in doc.Elements("GoodreadsResponse").Elements("user")
                               select new Profile
                               {
                                   Id = Convert.ToInt32((string)p.Element("id")),
                                   Name = (string)p.Element("name"),
                                   ImageUrl = (string)p.Element("image_url"),
                                   Link = (string)p.Element("link"),
                                   Website = (string)p.Element("website"),
                                   Age = GetXElementIntValue(p, "Age"),
                                   Gender = (string)p.Element("gender"),
                                   Location = (string)p.Element("location"),
                                   FavoriteAuthors = new List<Author>(
                                       from a in p.Element("favorite_authors").Elements()
                                       select new Author
                                       {
                                           Id = Convert.ToInt32((string)a.Element("id")),
                                           Name = (string)a.Element("name")
                                       }),
                                   FavoriteBooks = (string)p.Element("favorite_books"),
                                   Joined = DateTime.Parse((string)p.Element("joined")),
                                   Interests = (string)p.Element("interests"),
                                   About = HtmlUtilities.ConvertToText((string)p.Element("about")),
                                   Shelves = new List<Shelf>(
                                       from s in p.Element("user_shelves").Elements()
                                       select new Shelf
                                       {
                                           Id = Convert.ToInt32((string)s.Element("id")),
                                           Name = (string)s.Element("name"),
                                           Count = Convert.ToInt32((string)s.Element("book_count")),
                                           Description = (string)s.Element("description"),
                                           Exclusive = Convert.ToBoolean((string)s.Element("exclusive_flag"))
                                       }),

                                   Updates = new List<Update>(
                                       from u in p.Element("updates").Elements()
                                       select new Update
                                       {
                                           Type = ParseUpdateType((string)u.Attribute("type")),
                                           Text = HtmlUtilities.ConvertToText((string)u.Element("action_text")),
                                           Link = (string)u.Element("link"),
                                           ImageUrl = (string)u.Element("image_url"),
                                           UpdateTime = DateTime.Parse((string)u.Element("updated_at")),
                                           User = new Profile()
                                           {
                                               Id = Convert.ToInt32((string)u.Element("actor").Element("id")),
                                               Name = (string)u.Element("actor").Element("name")
                                           },

                                           Book = (from b in u.Elements("object").Elements("book")
                                                   select new Book
                                                   {
                                                       Id = (int)b.Element("id"),
                                                       Title = (string)b.Element("title")
                                                   }).SingleOrDefault(),

                                           Status = (from b in u.Elements("object").Elements("user-status")
                                                     select new UserStatus
                                                     {
                                                         Id = (int)b.Element("id"),
                                                         Body = (string)b.Element("body"),
                                                         BookId = (int)b.Element("book-id"),
                                                         Page = (int)b.Element("page"),
                                                         Percent = (int)b.Element("percent"),
                                                     }).SingleOrDefault()

                                       }),

                               }).Single();


            return profile;
        }

        public static int GetXElementIntValue(XElement pElement, string pstrElementName)
        {
            int value = 0;
            try
            {
                XElement lElement = pElement.Element(pstrElementName);
                if (lElement != null)
                {
                    value = Convert.ToInt32(lElement.Value);
                }
            }
            catch { }
            return value;
        }

        public static DateTime ParseGRDate(String value)
        {
            if (value == null || value.Length == 0)
                return DateTime.MinValue;

            CultureInfo provider = CultureInfo.InvariantCulture;
            return DateTime.ParseExact(value, "ddd MMM d HH:mm:ss zzzz yyyy", provider);
        }

        public static DateTime ParseCreatedAtDate(String value)
        {
            if (value == null || value.Length == 0)
                return DateTime.MinValue;

            CultureInfo provider = CultureInfo.InvariantCulture;

            //2013-01-24 T10:09:44-08:00
            return DateTime.ParseExact(value, "yyyy-MM-dd\\THH:mm:sszzz", provider);
        }

        public static Update.Actions ParseUpdateType(String value)
        {
            try
            {
                return (Update.Actions)Enum.Parse(typeof(Update.Actions), value, true);
            }
            catch (Exception e)
            {
                return Update.Actions.unknown;
            }
        }

        public static ReviewSet ParseShelf(String input)
        {
            XDocument doc = XDocument.Parse(input);
            ReviewSet page = (from p in doc.Elements("GoodreadsResponse").Elements("reviews")
                              select new ReviewSet
                              {
                                  Start = Convert.ToInt32((string)p.Attribute("start")),
                                  End = Convert.ToInt32((string)p.Attribute("end")),
                                  Total = Convert.ToInt32((string)p.Attribute("total")),

                                  Reviews = new List<Review>(
                                      from r in p.Elements()
                                      select new Review
                                      {
                                          Id = Convert.ToInt32((string)r.Element("id")),
                                          Rating = Convert.ToInt32((string)r.Element("rating")),
                                          Spoilers = Convert.ToBoolean((string)r.Element("spoiler_flag")),

                                          StartedAt = ParseGRDate((string)r.Element("started_at")),
                                          ReadAt = ParseGRDate((string)r.Element("read_at")),
                                          DateAdded = ParseGRDate((string)r.Element("date_added")),
                                          DateUpdated = ParseGRDate((string)r.Element("date_updated")),

                                          Url = (string)r.Element("url"),
                                          Body = (string)r.Element("body"),

                                          Book = new Book()
                                          {
                                              Id = Convert.ToInt32((string)r.Element("book").Element("id")),
                                              Title = (string)r.Element("book").Element("title"),
                                              Description = (string)r.Element("book").Element("description"),
                                              ImageUrl = (string)r.Element("book").Element("image_url"),
                                              SmallImageUrl = (string)r.Element("book").Element("small_image_url"),
                                              BookUrl = (string)r.Element("book").Element("link"),
                                              Publisher = (string)r.Element("book").Element("publisher"),
                                              ISBN = (string)r.Element("book").Element("isbn"),
                                              AvgRating = Convert.ToDouble((string)r.Element("book").Element("average_rating")),

                                              Authors = new List<Author>(
                                                  from a in r.Element("book").Element("authors").Elements()
                                                  select new Author
                                                  {
                                                      Id = Convert.ToInt32((string)a.Element("id")),
                                                      Name = (string)a.Element("name"),
                                                      ImageUrl = (string)a.Element("image_url"),
                                                      SmallImageUrl = (string)a.Element("small_image_url"),
                                                      Link = (string)a.Element("link")
                                                  }),
                                          },

                                          Shelves = r.Element("shelves").Elements().Select(x => x.Attribute("name").Value).ToList()

                                      }),

                              }).First();

            return page;
        }


        public static Book ParseBook(String input)
        {
            XDocument doc = XDocument.Parse(input);

            Book book = (from b in doc.Elements("GoodreadsResponse").Elements("book")
                         select new Book
                         {
                             Id = Convert.ToInt32((string)b.Element("id")),
                             Title = (string)b.Element("title"),
                             Description = HtmlUtilities.ConvertToText((string)b.Element("description")),
                             ImageUrl = (string)b.Element("image_url"),
                             SmallImageUrl = (string)b.Element("small_image_url"),
                             BookUrl = (string)b.Element("link"),
                             Publisher = (string)b.Element("publisher"),
                             ISBN = (string)b.Element("isbn"),
                             AvgRating = Convert.ToDouble((string)b.Element("average_rating")),

                             Authors = new List<Author>(
                                 from a in b.Element("authors").Elements()
                                 select new Author
                                 {
                                     Id = Convert.ToInt32((string)a.Element("id")),
                                     Name = (string)a.Element("name"),
                                     ImageUrl = (string)a.Element("image_url"),
                                     SmallImageUrl = (string)a.Element("small_image_url"),
                                     Link = (string)a.Element("link")
                                 }),

                             PublicationYear = GetXElementIntValue(b, "publication_year"),
                             PublicationMonth = GetXElementIntValue(b, "publication_month"),
                             PublicationDay = GetXElementIntValue(b, "publication_day"),

                             MyReview = new Review()
                                {
                                Id = Convert.ToInt32((string)b.Elements("my_review").Elements("id").FirstOrDefault()),
                                Shelves = b.Elements("my_review").Elements("shelves").Elements().Select(x => x.Attribute("name").Value).ToList(),

                                StartedAt   = ParseGRDate((string)b.Elements("my_review").Elements("started_at").FirstOrDefault()),
                                ReadAt      = ParseGRDate((string)b.Elements("my_review").Elements("read_at").FirstOrDefault()),
                                DateAdded   = ParseGRDate((string)b.Elements("my_review").Elements("date_added").FirstOrDefault()),
                                DateUpdated = ParseGRDate((string)b.Elements("my_review").Elements("date_updated").FirstOrDefault()),
                                },

                             Reviews = new List<Review>(
                                 from r in b.Element("reviews").Elements()
                                 select new Review
                                 {
                                     Id = Convert.ToInt32((string)r.Element("id")),
                                     Rating = Convert.ToInt32((string)r.Element("rating")),
                                     Spoilers = Convert.ToBoolean((string)r.Element("spoiler_flag")),

                                     StartedAt = ParseGRDate((string)r.Element("started_at")),
                                     ReadAt = ParseGRDate((string)r.Element("read_at")),
                                     DateAdded = ParseGRDate((string)r.Element("date_added")),
                                     DateUpdated = ParseGRDate((string)r.Element("date_updated")),

                                     Url = (string)r.Element("url"),
                                     Body = HtmlUtilities.ConvertToText((string)r.Element("body")),

                                     Shelves = r.Element("shelves").Elements().Select(x => x.Attribute("name").Value).ToList(),

                                     Reviewer = new Profile()
                                     {
                                         Id = Convert.ToInt32((string)r.Element("user").Element("id")),
                                         Name = (string)r.Element("user").Element("name"),
                                         Location = (string)r.Element("user").Element("location"),
                                         Link = (string)r.Element("user").Element("link"),
                                         ImageUrl = (string)r.Element("user").Element("image_url"),
                                     }

                                 }).ToList(),

                             FriendReviews = new List<Review>(
                                 from r in b.Elements("friend_reviews").Elements()
                                 select new Review
                                 {
                                     Id = Convert.ToInt32((string)r.Element("id")),
                                     Rating = Convert.ToInt32((string)r.Element("rating")),
                                     Spoilers = Convert.ToBoolean((string)r.Element("spoiler_flag")),

                                     StartedAt = ParseGRDate((string)r.Element("started_at")),
                                     ReadAt = ParseGRDate((string)r.Element("read_at")),
                                     DateAdded = ParseGRDate((string)r.Element("date_added")),
                                     DateUpdated = ParseGRDate((string)r.Element("date_updated")),

                                     Url = (string)r.Element("url"),
                                     Body = HtmlUtilities.ConvertToText((string)r.Element("body")),

                                     Shelves = r.Element("shelves").Elements().Select(x => x.Attribute("name").Value).ToList(),

                                     Reviewer = new Profile()
                                     {
                                         Id = Convert.ToInt32((string)r.Element("user").Element("id")),
                                         Name = (string)r.Element("user").Element("name"),
                                         Location = (string)r.Element("user").Element("location"),
                                         Link = (string)r.Element("user").Element("link"),
                                         ImageUrl = (string)r.Element("user").Element("image_url"),
                                     }

                                 }).ToList()


                         }).Single();

            return book;
        }

        public static int ParseOptionalInt(String input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

            return int.Parse(input);
        }


        public static Review ParseReview(String input)
        {
            XDocument doc = XDocument.Parse(input);
            Review review = (from r in doc.Elements("GoodreadsResponse").Elements("review")
                             select new Review
                             {

                                 Id = Convert.ToInt32((string)r.Element("id")),
                                 Rating = Convert.ToInt32((string)r.Element("rating")),
                                 Spoilers = Convert.ToBoolean((string)r.Element("spoiler_flag")),

                                 StartedAt = ParseGRDate((string)r.Element("started_at")),
                                 ReadAt = ParseGRDate((string)r.Element("read_at")),
                                 DateAdded = ParseGRDate((string)r.Element("date_added")),
                                 DateUpdated = ParseGRDate((string)r.Element("date_updated")),

                                 Url = (string)r.Element("url"),
                                 Body = HtmlUtilities.ConvertToText((string)r.Element("body")),

                                 CurrentPage = ParseOptionalInt((string)r.Elements("user_statuses").Elements("user_status").Elements("page").FirstOrDefault()),
                                 CurrentPercent = ParseOptionalInt((string)r.Elements("user_statuses").Elements("user_status").Elements("percent").FirstOrDefault()),

                                 Shelves = r.Element("shelves").Elements().Select(x => x.Attribute("name").Value).ToList(),

                                 Book = new Book()
                                 {
                                     Id = Convert.ToInt32((string)r.Element("book").Element("id")),
                                     Title = (string)r.Element("book").Element("title"),
                                     ImageUrl = (string)r.Element("book").Element("image_url"),

                                     Authors = new List<Author>(
                                         from a in r.Element("book").Element("authors").Elements()
                                         select new Author
                                         {
                                             Id = Convert.ToInt32((string)a.Element("id")),
                                             Name = (string)a.Element("name")
                                         }),
                                 },

                                 Reviewer = new Profile()
                                 {
                                     Id = Convert.ToInt32((string)r.Element("user").Element("id")),
                                     Name = (string)r.Element("user").Element("name")
                                 },

                                 Comments = new List<Review.Comment>(
                                     from c in r.Elements("comments").Elements("comment")
                                     select new Review.Comment
                                     {
                                         Id = (int)c.Element("id"),
                                         Body = HtmlUtilities.ConvertToText((string)c.Element("body")),
                                         User = new Profile()
                                         {
                                             Id = (int)c.Element("user").Element("id"),
                                             Name = (string)c.Element("user").Element("name"),
                                             Location = (string)c.Element("user").Element("location"),
                                             Link = (string)c.Element("user").Element("link"),
                                             ImageUrl = (string)c.Element("user").Element("image_url")
                                         },
                                         CreatedAt = ParseGRDate((string)c.Element("created_at")),
                                         UpdatedAt = ParseGRDate((string)c.Element("updated_at")),
                                     })

                             }).Single();

            return review;
        }

        public static DateTime ParseAuthorDate(String value)
        {
            if (value == null || value.Length == 0)
                return DateTime.MinValue;

            CultureInfo provider = CultureInfo.InvariantCulture;
            return DateTime.ParseExact(value, "yyyy/MM/dd", provider);
        }

        public static Author ParseAuthor(String input)
        {
            XDocument doc = XDocument.Parse(input);
            Author author = (from a in doc.Elements("GoodreadsResponse").Elements("author")
                             select new Author
                             {
                                 Id = (int)a.Element("id"),
                                 Name = (string)a.Element("name"),
                                 ImageUrl = (string)a.Element("image_url"),
                                 SmallImageUrl = (string)a.Element("small_image_url"),
                                 Link = (string)a.Element("link"),


                                 FanCount = (int)a.Element("fans_count"),
                                 WorksCount = (int)a.Element("works_count"),
                                 About = HtmlUtilities.ConvertToText((string)a.Element("about")),
                                 Influences = HtmlUtilities.ConvertToText((string)a.Element("influences")),
                                 Gender = (string)a.Element("gender"),
                                 Hometown = (string)a.Element("hometown"),

                                 Born = ParseAuthorDate((string)a.Element("born_at")),
                                 Died = ParseAuthorDate((string)a.Element("died_at")),

                                 Books = new List<Book>(
                                     from b in a.Element("books").Elements()
                                     select new Book
                                     {
                                         Id = (int)b.Element("id"),
                                         Title = (string)b.Element("title"),
                                         Description = (string)b.Element("description"),
                                         ImageUrl = (string)b.Element("image_url"),
                                         SmallImageUrl = (string)b.Element("small_image_url"),
                                         BookUrl = (string)b.Element("link"),
                                         Publisher = (string)b.Element("publisher"),
                                         ISBN = (string)b.Element("isbn"),
                                         AvgRating = (double)b.Element("average_rating"),
                                         PublicationYear = GetXElementIntValue(b, "publication_year"),
                                         PublicationMonth = GetXElementIntValue(b, "publication_month"),
                                         PublicationDay = GetXElementIntValue(b, "publication_day"),

                                     })

                             }).Single();

            return author;
        }




        public static List<Update> ParseUpdates(String input)
        {
            XDocument doc = XDocument.Parse(input);
            List<Update> updates = (from u in doc.Elements("GoodreadsResponse").Elements("updates").Elements("update")
                                    select new Update
                                    {
                                        Type = ParseUpdateType((string)u.Attribute("type")),
                                        Text = HtmlUtilities.ConvertToText((string)u.Element("action_text")),
                                        Body = (string)u.Elements("body").FirstOrDefault(),
                                        Link = (string)u.Element("link"),
                                        ImageUrl = (string)u.Element("image_url"),
                                        UpdateTime = DateTime.Parse((string)u.Element("updated_at")),
                                        User = new Profile()
                                        {
                                            Id = Convert.ToInt32((string)u.Element("actor").Element("id")),
                                            Name = (string)u.Element("actor").Element("name")
                                        },

                                        Book = (from b in u.Elements("object").Elements("book")
                                                select new Book
                                                {
                                                    Id = (int)b.Element("id"),
                                                    Title = (string)b.Element("title")
                                                }).SingleOrDefault(),

                                        Status = (from b in u.Elements("object").Elements("user-status")
                                                  select new UserStatus
                                                  {
                                                      Id = (int)b.Element("id"),
                                                      Body = (string)b.Element("body"),
                                                      BookId = (int)b.Element("book-id"),
                                                      Page = (int)b.Element("page"),
                                                      Percent = (int)b.Element("percent"),
                                                  }).SingleOrDefault()


                                    }).ToList();

            return updates;
            //Console.WriteLine("adsad");
        }

        public static FriendSet ParseFriends(String input)
        {
            XDocument doc = XDocument.Parse(input);
            FriendSet friends = (from s in doc.Elements("GoodreadsResponse").Elements("friends")
                                     select new FriendSet
                                     {
                                         Start  = Convert.ToInt32((string)s.Attribute("start")),
                                         End    = Convert.ToInt32((string)s.Attribute("end")),
                                         Total  = Convert.ToInt32((string)s.Attribute("total")),

                                         Friends = new List<Profile>(
                                             from p in s.Elements("user")
                                             select new Profile
                                             {
                                                 Id = Convert.ToInt32((string)p.Element("id")),
                                                 Name = (string)p.Element("name"),
                                                 ImageUrl = (string)p.Element("image_url"),
                                                 Link = (string)p.Element("link")

                                             }),

                                     }).First();
            return friends;
        }

        public static BookSet ParseAuthorShelf(String input)
        {
            XDocument doc = XDocument.Parse(input);
            BookSet page = (from shelf in doc.Elements("GoodreadsResponse").Elements("author").Elements("books")
                            select new BookSet
                            {
                                Start = (int)shelf.Attribute("start"),
                                End = (int)shelf.Attribute("end"),
                                Total = (int)shelf.Attribute("total"),

                                Books = new List<Book>(
                                    from b in shelf.Elements()
                                    select new Book
                                    {
                                        Id = (int)b.Element("id"),
                                        Title = (string)b.Element("title"),
                                        Description = (string)b.Element("description"),
                                        ImageUrl = (string)b.Element("image_url"),
                                        SmallImageUrl = (string)b.Element("small_image_url"),
                                        BookUrl = (string)b.Element("link"),
                                        Publisher = (string)b.Element("publisher"),
                                        ISBN = (string)b.Element("isbn"),
                                        AvgRating = (double)b.Element("average_rating"),

                                    })

                            }).First();
            return page;
        }

        public static List<Book> ParseSearchResults(String input)
        {
            XDocument doc = XDocument.Parse(input);
            List<Book> books = (from b in doc.Elements("GoodreadsResponse").Elements("search").Elements("results").Elements("work")
                                select new Book
                                {
                                    PublicationYear = GetXElementIntValue(b, "original_publication_year"),
                                    PublicationMonth = GetXElementIntValue(b, "original_publication_month"),
                                    PublicationDay = GetXElementIntValue(b, "original_publication_day"),

                                    AvgRating = (double)b.Element("average_rating"),

                                    Id = (int)b.Element("best_book").Element("id"),
                                    Title = (string)b.Element("best_book").Element("title"),
                                    ImageUrl = (string)b.Element("best_book").Element("image_url"),
                                    SmallImageUrl = (string)b.Element("best_book").Element("small_image_url"),

                                    Authors = new List<Author>(
                                        from a in b.Element("best_book").Elements("author")
                                        select new Author
                                        {
                                            Id = Convert.ToInt32((string)a.Element("id")),
                                            Name = (string)a.Element("name")
                                        }),

                                }).ToList();

            return books;
        }

        public static int ParseAuthResponse(String input)
        {
            XDocument doc = XDocument.Parse(input);
            return (int)doc.Element("GoodreadsResponse").Element("user").Attribute("id");
        }


        public static void ParseEvents()
        {
            XDocument doc = XDocument.Load(@"C:\Users\Daniel.Delaney\Downloads\events.xml");
            List<Event> events = (from e in doc.Elements("GoodreadsResponse").Elements("events").Elements("event")
                                  select new Event
                                  {
                                      Id = (int)e.Element("id"),
                                      Title = (string)e.Element("title"),
                                      Venue = (string)e.Element("venue"),
                                      Link = (string)e.Element("link"),
                                      Description = (string)e.Element("description"),
                                      ImageUrl = (string)e.Element("image_url"),
                                      City = (string)e.Element("city"),
                                      StateCode = (string)e.Element("state_code"),
                                      CountryCode = (string)e.Element("country_code"),
                                      EventType = (string)e.Element("event_type"),
                                      StartAt = (DateTime)e.Element("start_at"),
                                  }).ToList();

            //Console.WriteLine("adsad");
        }

        public static List<Group> ParseGroups(String input)
        {
            XDocument doc = XDocument.Parse(input);
            List<Group> groups = (from g in doc.Elements("GoodreadsResponse").Elements("groups").Elements("list").Elements("group")
                                  select new Group
                                  {
                                      Id = (int)g.Element("id"),
                                      UserCount = (int)g.Element("users_count"),
                                      Title = (string)g.Element("title"),
                                      Access = (string)g.Element("access"),
                                      ImageUrl = (string)g.Element("image_url"),
                                      LastActivity = ParseGRDate((string)g.Element("last_activity_at"))
                                  }).ToList();

            return groups;
        }

        private static int ParseGroupId(String input)
        {
            if (input.Equals("unread"))
                return -1;

            return Int32.Parse(input);
        }

        public static Group ParseGroup(String input)
        {
            XDocument doc = XDocument.Parse(input);
            Group group = (from g in doc.Elements("GoodreadsResponse").Elements("group")
                           select new Group
                           {
                               Id = (int)g.Element("id"),
                               UserCount = (int)g.Element("group_users_count"),
                               Title = (string)g.Element("title"),
                               Access = (string)g.Element("access"),
                               ImageUrl = (string)g.Element("image_url"),
                               LastActivity = ParseGRDate((string)g.Element("last_activity_at")),
                               Category = (string)g.Element("category"),
                               SubCategory = (string)g.Element("subcategory"),
                               Rules = (string)g.Element("rules"),
                               Link = (string)g.Element("link"),
                               Description = HtmlUtilities.ConvertToText((string)g.Element("description")),
                               Location = (string)g.Element("location"),

                               Folders = new List<Group.Folder>(
                                   from f in g.Element("folders").Elements("folder")
                                   select new Group.Folder
                                   {
                                       Id = ParseGroupId((string)f.Element("id")),
                                       Name = HtmlUtilities.ConvertToText((string)f.Element("name")),
                                       TopicCount = (int)f.Element("items_count"),
                                       UpdatedAt = ParseGRDate((string)f.Element("updated_at")),
                                   }),

                           }).Single();

            if (group != null && group.Folders != null)
            {
                foreach (Group.Folder f in group.Folders)
                {
                    if (f.Name.Equals("unread"))
                    {
                        group.Folders.Remove(f);
                        return group;
                    }

                }
            }

            return group;
        }

        public static TopicSet ParseTopics(String input)
        {
            XDocument doc = XDocument.Parse(input);
            TopicSet topics = (from s in doc.Elements("GoodreadsResponse").Elements("group_folder").Elements("topics")
                               select new TopicSet
                               {
                                   Start = Convert.ToInt32((string)s.Attribute("start")),
                                   End = Convert.ToInt32((string)s.Attribute("end")),
                                   Total = Convert.ToInt32((string)s.Attribute("total")),

                                   Topics = new List<Topic>(
                                       from t in s.Elements("topic")
                                       select new Topic
                                       {
                                           Id = (int)t.Element("id"),
                                           CommentCount = (int)t.Element("comments_count"),
                                           LastCommentAt = (DateTime)t.Element("last_comment_at"),
                                           Title = (string)t.Element("title"),
                                           Author = (string)t.Elements("author_user").Elements("first_name").FirstOrDefault(),
                                       }),

                               }).First();

            return topics;
        }

        public static Topic ParseTopic(String input)
        {
            XDocument doc = XDocument.Parse(input);
            Topic topic = (from t in doc.Elements("GoodreadsResponse").Elements("topic")
                           select new Topic
                           {
                               Id = (int)t.Element("id"),
                               CommentCount = (int)t.Element("comments_count"),
                               LastCommentAt = ParseGRDate((string)t.Element("last_comment_at")),
                               Title = (string)t.Element("title"),

                               Comments = new List<Topic.Comment>(
                                   from c in t.Elements("comments").Elements("comment")
                                   select new Topic.Comment
                                   {
                                       Id = (int)c.Element("id"),
                                       Body = HtmlUtilities.ConvertToText((string)c.Element("body")),
                                       UpdatedAt = ParseGRDate((string)c.Element("updated_at")),

                                       User = new Profile()
                                       {
                                           Id = (int)c.Element("user").Element("id"),
                                           Name = (string)c.Element("user").Element("name")
                                       },

                                   }),

                           }).First();
            return topic;
        }

        public static Status ParseStatus(String input)
        {
            XDocument doc = XDocument.Parse(input);
            Status status = (from s in doc.Elements("GoodreadsResponse").Elements("user_status")
                             select new Status
                             {

                                 Id = Convert.ToInt32((string)s.Element("id")),
                                 Header = HtmlUtilities.ConvertToText((string)s.Element("header")),
                                 Body = HtmlUtilities.ConvertToText((string)s.Element("body")),
                                 CreatedAt = ParseCreatedAtDate((string)s.Element("created_at")),
                                 LikeCount = Convert.ToInt32((string)s.Element("likes_count")),

                                 User = new Profile()
                                 {
                                     Id = Convert.ToInt32((string)s.Element("user").Element("id")),
                                     Name = (string)s.Element("user").Element("name"),
                                     ImageUrl = (string)s.Element("user").Element("image_url"),
                                 },

                                 Comments = new List<Status.Comment>(
                                     from c in s.Elements("comments").Elements("comment")
                                     select new Status.Comment
                                     {
                                         Id = (int)c.Element("id"),
                                         Body = HtmlUtilities.ConvertToText((string)c.Element("body")),
                                         User = new Profile()
                                         {
                                             Id = (int)c.Element("user").Element("id"),
                                             Name = (string)c.Element("user").Element("name"),
                                             ImageUrl = (string)c.Element("user").Element("small_image_url")
                                         },
                                         UpdatedAt = ParseGRDate((string)c.Element("updated_at")),
                                     })

                             }).Single();

            return status;
        }

        public static List<Comparison> ParseComparison(String input)
        {
            XDocument doc = XDocument.Parse(input);
            List<Comparison> comp = (from c in doc.Elements("GoodreadsResponse").Elements("compare").Elements("reviews").Elements("review")
                                     select new Comparison
                                     {
                                         Book = new Book()
                                         {
                                             Id = (int)c.Element("book").Element("id"),
                                             Title = (string)c.Element("book").Element("title"),
                                         },

                                         MyReview = new Review()
                                         {
                                             Rating = ParseComparisonRating((string)c.Elements("your_review").Elements("rating").FirstOrDefault()),
                                             Shelves = ParseComparisonShelf((string)c.Elements("your_review").Elements("rating").FirstOrDefault()),
                                         },

                                         TheirReview = new Review()
                                         {
                                             Rating = ParseComparisonRating((string)c.Elements("their_review").Elements("rating").FirstOrDefault()),
                                             Shelves = ParseComparisonShelf((string)c.Elements("their_review").Elements("rating").FirstOrDefault())
                                         }


                                     }).ToList();

            return comp;
        }

        private static int ParseComparisonRating(String input)
        {
            int number;
            if (Int32.TryParse(input, out number))
                return number;

            return 0;
        }

        private static List<String> ParseComparisonShelf(String input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            int number;
            if (Int32.TryParse(input, out number))
                return null;

            List<String> shelves = new List<string>();
            shelves.Add(input);
            return shelves;
        }
    }
}
