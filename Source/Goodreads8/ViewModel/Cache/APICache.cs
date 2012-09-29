using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goodreads8.ViewModel.Cache
{
    class APICache
    {
        private Dictionary<ShelfCache, ReviewSet> m_shelfCache;
        private Dictionary<WorksCache, BookSet> m_authorShelfCache;
        private Dictionary<int, Book> m_bookCache;
        private Dictionary<int, Review> m_reviewCache;
        private Dictionary<int, Author> m_authorCache;
        private Dictionary<int, Profile> m_userCache;
        private Dictionary<int, Group> m_groupCache;
        private Dictionary<int, Topic> m_topicCache;
        private Dictionary<int, Status> m_statusCache;
        private Dictionary<FriendCache, FriendSet> m_friendsCache;
        private Dictionary<UserReviewCache, Review> m_userReviewCache;
        private Dictionary<TopicSetCache, TopicSet> m_topicListCache;

        private List<Update> m_updates = null;
        private DateTime m_updateExpiration = DateTime.MinValue;

        private List<Group> m_groups = null;
        private DateTime m_groupExpiration = DateTime.MinValue;

        public APICache()
        {
            m_shelfCache = new Dictionary<ShelfCache, ReviewSet>();
            m_bookCache = new Dictionary<int, Book>();
            m_reviewCache = new Dictionary<int, Review>();
            m_authorCache = new Dictionary<int, Author>();
            m_userCache = new Dictionary<int, Profile>();
            m_friendsCache = new Dictionary<FriendCache, FriendSet>();
            m_userReviewCache = new Dictionary<UserReviewCache, Review>();
            m_groupCache = new Dictionary<int, Group>();
            m_topicListCache = new Dictionary<TopicSetCache, TopicSet>();
            m_topicCache = new Dictionary<int, Topic>();
            m_authorShelfCache = new Dictionary<WorksCache, BookSet>();
            m_statusCache = new Dictionary<int, Status>();
        }

        public void InvalidateMyReview(int userId, int bookId)
        {
            foreach (KeyValuePair<UserReviewCache, Review> pair in m_userReviewCache)
            {
                if (pair.Key.UserId == userId &&
                    pair.Key.BookId == bookId)
                {
                    m_userReviewCache.Remove(pair.Key);
                    return;
                }
            }
        }

        private void PruneCache()
        {
            List<ShelfCache> shelfKeys = new List<ShelfCache>();
            foreach (KeyValuePair<ShelfCache, ReviewSet> pair in m_shelfCache)
            {
                if (pair.Key.IsExpired())
                    shelfKeys.Add(pair.Key);
            }
            foreach (ShelfCache key in shelfKeys)
            {
                m_shelfCache.Remove(key);
            }

            //Authors
            List<WorksCache> authorKeys = new List<WorksCache>();
            foreach (KeyValuePair<WorksCache, BookSet> pair in m_authorShelfCache)
            {
                if (pair.Key.IsExpired())
                    authorKeys.Add(pair.Key);
            }
            foreach (WorksCache key in authorKeys)
            {
                m_authorShelfCache.Remove(key);
            }
        }

        /** -----------------------
         *
         *       Shelf Contents
         *
         * ----------------------- */
        public ReviewSet GetShelfContents(int userId, String shelf, int page, String sort, String order)
        {
            PruneCache();

            foreach (KeyValuePair<ShelfCache, ReviewSet> pair in m_shelfCache)
            {
                if (pair.Key.Shelf.Equals(shelf) &&
                    pair.Key.Sort.Equals(sort) &&
                    pair.Key.Order.Equals(order) &&
                    pair.Key.Page == page &&
                    pair.Key.UserId == userId)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public void StoreShelfContents(int userId, String shelf, int page, String sort, String order, ReviewSet data)
        {
            ShelfCache cache = new ShelfCache();
            cache.UserId = userId;
            cache.Shelf = shelf;
            cache.Page = page;
            cache.Sort = sort;
            cache.Order = order;
            m_shelfCache.Add(cache, data);
        }

        /** -----------------------
         *
         *       Books
         *
         * ----------------------- */
        public Book GetBook(int bookId)
        {
            if (m_bookCache.ContainsKey(bookId))
                return m_bookCache[bookId];
            return null;
        }

        public void StoreBook(int bookId, Book data)
        {
            m_bookCache.Add(bookId, data);
        }

        /** -----------------------
         *
         *       Reviews
         *
         * ----------------------- */
        public Review GetReview(int reviewId)
        {
            if (m_reviewCache.ContainsKey(reviewId))
                return m_reviewCache[reviewId];
            return null;
        }

        public void StoreReview(int reviewId, Review data)
        {
            m_reviewCache.Add(reviewId, data);
        }

        public void InvalidateReview(int reviewId)
        {
            if (m_reviewCache.ContainsKey(reviewId))
                m_reviewCache.Remove(reviewId);
        }

        /** -----------------------
         *
         *       Authors
         *
         * ----------------------- */
        public Author GetAuthor(int authorId)
        {
            if (m_authorCache.ContainsKey(authorId))
                return m_authorCache[authorId];
            return null;
        }

        public void StoreAuthor(int authorId, Author data)
        {
            m_authorCache.Add(authorId, data);
        }

        /** -----------------------
         *
         *       Profile
         *
         * ----------------------- */
        public Profile GetProfile(int userId)
        {
            if (m_userCache.ContainsKey(userId))
                return m_userCache[userId];
            return null;
        }

        public void StoreProfile(int userId, Profile data)
        {
            m_userCache.Add(userId, data);
        }

        /** -----------------------
         *
         *       Friends
         *
         * ----------------------- */
        public FriendSet GetFriends(int userId, int page)
        {
            foreach (KeyValuePair<FriendCache, FriendSet> pair in m_friendsCache)
            {
                if (pair.Key.Page == page &&
                    pair.Key.UserId == userId)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public void StoreFriends(int userId, int page, FriendSet data)
        {
            FriendCache cache = new FriendCache();
            cache.UserId = userId;
            cache.Page = page;
            m_friendsCache.Add(cache, data);
        }

        /** -----------------------
         *
         *       Updates
         *
         * ----------------------- */
        public List<Update> GetUpdates()
        {
            if (m_updates != null && m_updateExpiration.CompareTo(DateTime.Now) < 0)
            {
                m_updates = null;
                m_updateExpiration = DateTime.MinValue;
                return null;
            }

            return m_updates;
        }

        public void StoreUpdates(List<Update> data)
        {
            m_updates = data;
            m_updateExpiration = DateTime.Now.AddHours(2);
        }

        /** -----------------------
         *
         *       User Review
         *
         * ----------------------- */
        public Review GetUserReview(int bookId, int userId)
        {
            foreach (KeyValuePair<UserReviewCache, Review> pair in m_userReviewCache)
            {
                if (pair.Key.UserId == userId &&
                    pair.Key.BookId == bookId)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public void StoreUserReview(int bookId, int userId, Review data)
        {
            UserReviewCache cache = new UserReviewCache();
            cache.UserId = userId;
            cache.BookId = bookId;
            m_userReviewCache.Add(cache, data);
        }

        /** -----------------------
         *
         *       Groups
         *
         * ----------------------- */
        public List<Group> GetGroups()
        {
            if (m_groups != null && m_groupExpiration.CompareTo(DateTime.Now) < 0)
            {
                m_groups = null;
                m_groupExpiration = DateTime.MinValue;
                return null;
            }

            return m_groups;
        }

        public void StoreGroups(List<Group> data)
        {
            m_groups = data;
            m_groupExpiration = DateTime.Now.AddHours(2);
        }

        /** -----------------------
         *
         *       Group
         *
         * ----------------------- */
        public Group GetGroup(int groupId)
        {
            if (m_groupCache.ContainsKey(groupId))
                return m_groupCache[groupId];
            return null;
        }

        public void StoreGroup(int groupId, Group data)
        {
            m_groupCache.Add(groupId, data);
        }

        /** -----------------------
         *
         *       Topic Sets
         *
         * ----------------------- */
        public TopicSet GetTopicSet(int groupId, int folderId, int page)
        {
            foreach (KeyValuePair<TopicSetCache, TopicSet> pair in m_topicListCache)
            {
                if (pair.Key.GroupId == groupId &&
                    pair.Key.FolderId == folderId &&
                    pair.Key.Page == page)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public void StoreTopicSet(int groupId, int folderId, int page, TopicSet data)
        {
            TopicSetCache cache = new TopicSetCache();
            cache.GroupId = groupId;
            cache.FolderId = folderId;
            cache.Page = page;
            m_topicListCache.Add(cache, data);
        }

        /** -----------------------
         *
         *       Topic
         *
         * ----------------------- */
        public Topic GetTopic(int topicId)
        {
            if (m_topicCache.ContainsKey(topicId))
                return m_topicCache[topicId];
            return null;
        }

        public void StoreTopic(int topicId, Topic data)
        {
            m_topicCache.Add(topicId, data);
        }

        public void InvalidateTopic(int topicId)
        {
            if (m_topicCache.ContainsKey(topicId))
                m_topicCache.Remove(topicId);
        }

        /** -----------------------
         *
         *       Author works
         *
         * ----------------------- */
        public BookSet GetAuthorWorks(int authorId, int page)
        {
            PruneCache();

            foreach (KeyValuePair<WorksCache, BookSet> pair in m_authorShelfCache)
            {
                if (pair.Key.Page == page &&
                    pair.Key.AuthorId == authorId)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public void StoreAuthorWorks(int authorId, int page, BookSet data)
        {
            WorksCache cache = new WorksCache();
            cache.AuthorId = authorId;
            cache.Page = page;
            m_authorShelfCache.Add(cache, data);
        }

        /** -----------------------
         *
         *       Status
         *
         * ----------------------- */
        public Status GetStatus(int statusId)
        {
            if (m_statusCache.ContainsKey(statusId))
                return m_statusCache[statusId];
            return null;
        }

        public void StoreStatus(int statusId, Status data)
        {
            m_statusCache.Add(statusId, data);
        }

        public void InvalidateStatus(int statusId)
        {
            if (m_statusCache.ContainsKey(statusId))
                m_statusCache.Remove(statusId);
        }
    }
}
