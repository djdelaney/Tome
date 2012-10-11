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
        private Dictionary<int, CacheEntry> m_bookCache;
        private Dictionary<int, CacheEntry> m_reviewCache;
        private Dictionary<int, CacheEntry> m_authorCache;
        private Dictionary<int, CacheEntry> m_userCache;
        private Dictionary<int, CacheEntry> m_groupCache;
        private Dictionary<int, CacheEntry> m_topicCache;
        private Dictionary<int, CacheEntry> m_statusCache;
        private Dictionary<FriendCache, FriendSet> m_friendsCache;
        private Dictionary<UserReviewCache, Review> m_userReviewCache;
        private Dictionary<TopicSetCache, TopicSet> m_topicListCache;

        private List<Update> m_updates = null;
        private DateTime m_updateExpiration = DateTime.MinValue;

        private List<Group> m_groups = null;
        private DateTime m_groupExpiration = DateTime.MinValue;

        public APICache()
        {
            m_shelfCache 		= new Dictionary<ShelfCache, ReviewSet>();
            m_authorShelfCache 	= new Dictionary<WorksCache, BookSet>();
            m_bookCache 		= new Dictionary<int, CacheEntry>();
            m_reviewCache 		= new Dictionary<int, CacheEntry>();
            m_authorCache 		= new Dictionary<int, CacheEntry>();
            m_userCache 		= new Dictionary<int, CacheEntry>();
            m_groupCache 		= new Dictionary<int, CacheEntry>();
            m_topicCache 		= new Dictionary<int, CacheEntry>();
            m_statusCache 		= new Dictionary<int, CacheEntry>();
            m_friendsCache 		= new Dictionary<FriendCache, FriendSet>();
            m_userReviewCache 	= new Dictionary<UserReviewCache, Review>();
            m_topicListCache 	= new Dictionary<TopicSetCache, TopicSet>();
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
            //Shelf
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
            List<WorksCache> authorWorkKeys = new List<WorksCache>();
            foreach (KeyValuePair<WorksCache, BookSet> pair in m_authorShelfCache)
            {
                if (pair.Key.IsExpired())
                    authorWorkKeys.Add(pair.Key);
            }
            foreach (WorksCache key in authorWorkKeys)
            {
                m_authorShelfCache.Remove(key);
            }

            //Cache entry style
            //Book
            List<int> bookKeys = new List<int>();
            foreach (KeyValuePair<int, CacheEntry> pair in m_bookCache)
            {
                if (pair.Value.IsExpired())
                    bookKeys.Add(pair.Key);
            }
            foreach (int key in bookKeys)
            {
                m_bookCache.Remove(key);
            }

            //Reviews
            List<int> reviewKeys = new List<int>();
            foreach (KeyValuePair<int, CacheEntry> pair in m_reviewCache)
            {
                if (pair.Value.IsExpired())
                    reviewKeys.Add(pair.Key);
            }
            foreach (int key in reviewKeys)
            {
                m_reviewCache.Remove(key);
            }

            //Authors
            List<int> authorKeys = new List<int>();
            foreach (KeyValuePair<int, CacheEntry> pair in m_authorCache)
            {
                if (pair.Value.IsExpired())
                    authorKeys.Add(pair.Key);
            }
            foreach (int key in authorKeys)
            {
                m_authorCache.Remove(key);
            }

            //Users
            List<int> userKeys = new List<int>();
            foreach (KeyValuePair<int, CacheEntry> pair in m_userCache)
            {
                if (pair.Value.IsExpired())
                    userKeys.Add(pair.Key);
            }
            foreach (int key in userKeys)
            {
                m_userCache.Remove(key);
            }

            //Groups
            List<int> groupKeys = new List<int>();
            foreach (KeyValuePair<int, CacheEntry> pair in m_groupCache)
            {
                if (pair.Value.IsExpired())
                    groupKeys.Add(pair.Key);
            }
            foreach (int key in groupKeys)
            {
                m_groupCache.Remove(key);
            }

            //Topic
            List<int> topicKeys = new List<int>();
            foreach (KeyValuePair<int, CacheEntry> pair in m_topicCache)
            {
                if (pair.Value.IsExpired())
                    topicKeys.Add(pair.Key);
            }
            foreach (int key in topicKeys)
            {
                m_topicCache.Remove(key);
            }

            //Status
            List<int> statusKeys = new List<int>();
            foreach (KeyValuePair<int, CacheEntry> pair in m_statusCache)
            {
                if (pair.Value.IsExpired())
                    statusKeys.Add(pair.Key);
            }
            foreach (int key in statusKeys)
            {
                m_statusCache.Remove(key);
            }

            //pages of friends (invalidate all if any are expired)
            bool invalidateFriendCache = false;
            foreach (KeyValuePair<FriendCache, FriendSet> pair in m_friendsCache)
            {
                if (pair.Key.IsExpired())
                    invalidateFriendCache = true;
            }
            if (invalidateFriendCache)
            {
                m_friendsCache.Clear();
            }

            //User Reviews
            List<UserReviewCache> userReviewKeys = new List<UserReviewCache>();
            foreach (KeyValuePair<UserReviewCache, Review> pair in m_userReviewCache)
            {
                if (pair.Key.IsExpired())
                    userReviewKeys.Add(pair.Key);
            }
            foreach (UserReviewCache key in userReviewKeys)
            {
                m_userReviewCache.Remove(key);
            }

            //Topic List
            List<TopicSetCache> topicListKeys = new List<TopicSetCache>();
            foreach (KeyValuePair<TopicSetCache, TopicSet> pair in m_topicListCache)
            {
                if (pair.Key.IsExpired())
                    topicListKeys.Add(pair.Key);
            }
            foreach (TopicSetCache key in topicListKeys)
            {
                m_topicListCache.Remove(key);
            }

            //updates
            if (m_updates != null && m_updateExpiration.CompareTo(DateTime.Now) < 0)
            {
                m_updates = null;
                m_updateExpiration = DateTime.MinValue;
            }

            //Groups
            if (m_groups != null && m_groupExpiration.CompareTo(DateTime.Now) < 0)
            {
                m_groups = null;
                m_groupExpiration = DateTime.MinValue;
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
            PruneCache();

            if (m_bookCache.ContainsKey(bookId))
                return m_bookCache[bookId].Object as Book;
            return null;
        }

        public void StoreBook(int bookId, Book data)
        {
            m_bookCache.Add(bookId, new CacheEntry(data));
        }

        /** -----------------------
         *
         *       Reviews
         *
         * ----------------------- */
        public Review GetReview(int reviewId)
        {
            PruneCache();

            if (m_reviewCache.ContainsKey(reviewId))
                return m_reviewCache[reviewId].Object as Review;
            return null;
        }

        public void StoreReview(int reviewId, Review data)
        {
            m_reviewCache.Add(reviewId, new CacheEntry(data));
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
            PruneCache();

            if (m_authorCache.ContainsKey(authorId))
                return m_authorCache[authorId].Object as Author;
            return null;
        }

        public void StoreAuthor(int authorId, Author data)
        {
            m_authorCache.Add(authorId, new CacheEntry(data));
        }

        /** -----------------------
         *
         *       Profile
         *
         * ----------------------- */
        public Profile GetProfile(int userId)
        {
            PruneCache();

            if (m_userCache.ContainsKey(userId))
                return m_userCache[userId].Object as Profile;
            return null;
        }

        public void StoreProfile(int userId, Profile data)
        {
            m_userCache.Add(userId, new CacheEntry(data));
        }

        /** -----------------------
         *
         *       Friends
         *
         * ----------------------- */
        public FriendSet GetFriends(int userId, int page)
        {
            PruneCache();

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
            PruneCache();

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
            PruneCache();

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
            PruneCache();

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
            PruneCache();

            if (m_groupCache.ContainsKey(groupId))
                return m_groupCache[groupId].Object as Group;
            return null;
        }

        public void StoreGroup(int groupId, Group data)
        {
            m_groupCache.Add(groupId, new CacheEntry(data));
        }

        /** -----------------------
         *
         *       Topic Sets
         *
         * ----------------------- */
        public TopicSet GetTopicSet(int groupId, int folderId, int page)
        {
            PruneCache();

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
            PruneCache();

            if (m_topicCache.ContainsKey(topicId))
                return m_topicCache[topicId].Object as Topic;
            return null;
        }

        public void StoreTopic(int topicId, Topic data)
        {
            m_topicCache.Add(topicId, new CacheEntry(data));
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
            PruneCache();

            if (m_statusCache.ContainsKey(statusId))
                return m_statusCache[statusId].Object as Status;
            return null;
        }

        public void StoreStatus(int statusId, Status data)
        {
            m_statusCache.Add(statusId, new CacheEntry(data));
        }

        public void InvalidateStatus(int statusId)
        {
            if (m_statusCache.ContainsKey(statusId))
                m_statusCache.Remove(statusId);
        }
    }
}
