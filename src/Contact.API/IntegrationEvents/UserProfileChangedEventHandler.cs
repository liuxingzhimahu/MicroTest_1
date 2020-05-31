﻿using Contact.API.Data;
using DotNetCore.CAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Contact.API.IntegrationEvents
{
    public class UserProfileChangedEventHandler : ICapSubscribe
    {
        private IContactRepository _contactRepository;
        public UserProfileChangedEventHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [CapSubscribe("finbook.userapi.user_profile_changed")]
        public async Task UpdateContactInfo(UserProfileChangedEvent @event)
        {
            var token = new CancellationToken();

            await _contactRepository.UpdateContactInfoAsync(new Dtos.UserIdentity
            {
                UserId = @event.UserId,
                Name = @event.Name,
                Company = @event.Company,
                Title = @event.Title,
                Avatar = @event.Avatar
            }, token);

        }
    }
}
