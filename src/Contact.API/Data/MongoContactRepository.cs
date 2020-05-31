using Contact.API.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Contact.API.Models;

namespace Contact.API.Data
{
    public class MongoContactRepository : IContactRepository
    {
        private readonly ContactContext _contactContext;

        public MongoContactRepository(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }

        /// <summary>
        /// 添加联系人信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> AddContactAsync(int userId, UserIdentity userInfo, CancellationToken cancellationToken)
        {
            //根据当前用户id匹配，如果为空则用当前用户id新增一列，再将当前用户id匹配项的联系人信息更新
            if (await _contactContext.ContactBooks.CountDocumentsAsync(c => c.UserId == userId) == 0)
            {
                await _contactContext.ContactBooks.InsertOneAsync(new ContactBook() { UserId = userId });
            }

            var filter = Builders<ContactBook>.Filter.Eq(c => c.UserId, userId);
            var update = Builders<ContactBook>.Update.AddToSet(c=>c.Contacts, new Models.Contact() 
            {
                UserId = userInfo.UserId,
                Avatar = userInfo.Avatar,
                Company = userInfo.Company,
                Name = userInfo.Name,
                Title = userInfo.Title,
            });
            var result = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);
            return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
        }

        /// <summary>
        /// 获取联系人列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Models.Contact>> GetContactAsync(int userId, CancellationToken cancellationToken)
        {
            var contactBook = (await _contactContext.ContactBooks.FindAsync(c => c.UserId == userId)).FirstOrDefault(cancellationToken);
            if (contactBook != null)
            {
                return contactBook.Contacts;
            }
            //log tbd
            return new List<Models.Contact>();
        }

        /// <summary>
        /// 给联系人打标签
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="contactId"></param>
        /// <param name="tags"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken)
        {
            //多条件查询：先匹配当前用户Id和联系人Id，然后更新标签
            var filter = Builders<ContactBook>.Filter.And(
                Builders<ContactBook>.Filter.Eq(c => c.UserId, userId),
                Builders<ContactBook>.Filter.Eq("Contacts.UserId", contactId)
                );

            var update = Builders<ContactBook>.Update
                .Set("Contacts.$.Tags", tags);

            var result = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);
            return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
        }

        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> UpdateContactInfoAsync(UserIdentity userInfo, CancellationToken cancellationToken)
        {

            //根据当前用户Id匹配，再获取匹配项的所有联系人Id
            var contactBook = (await _contactContext.ContactBooks.FindAsync(c => c.UserId == userInfo.UserId, null, cancellationToken))
                .FirstOrDefault(cancellationToken);
            if (contactBook == null)
               // throw new Exception($"wrong user id for update contact info userid :{userInfo.UserId}");
               return true;
            var contactIds = contactBook.Contacts.Select(c => c.UserId);

            //然后再去更新各个联系人的 联系人中的当前用户(userid = userinfo.userid)
            var filter = Builders<ContactBook>.Filter.And(
                Builders<ContactBook>.Filter.In(c => c.UserId, contactIds),//查找联系人的联系人
                Builders<ContactBook>.Filter.ElemMatch(c => c.Contacts, contact => contact.UserId == userInfo.UserId)//联系人中当前用户
                );
            var update = Builders<ContactBook>.Update
                .Set("Contacts.$.Name", userInfo.Name)
                .Set("Contacts.$.Avator", userInfo.Avatar)
                .Set("Contacts.$.Company", userInfo.Company)
                .Set("Contacts.$.Title", userInfo.Title);

            var updateResult = _contactContext.ContactBooks.UpdateMany(filter, update);
            return updateResult.MatchedCount == updateResult.ModifiedCount;
        }
    }
}
