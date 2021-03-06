﻿using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class MemberGroupService : RepositoryService, IMemberGroupService
    {
        private readonly IMemberGroupRepository _memberGroupRepository;

        public MemberGroupService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IMemberGroupRepository memberGroupRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _memberGroupRepository = memberGroupRepository;
            //Proxy events!
            MemberGroupRepository.SavedMemberGroup += MemberGroupRepository_SavedMemberGroup;
            MemberGroupRepository.SavingMemberGroup += MemberGroupRepository_SavingMemberGroup;
        }

        #region Proxied event handlers

        void MemberGroupRepository_SavingMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
            // fixme - wtf?
            // why is the repository triggering these events?
            // and, the events are *dispatched* by the repository so it makes no sense dispatching them again!

            // v7.6
            //using (var scope = UowProvider.ScopeProvider.CreateScope())
            //{
            //    scope.Complete(); // always
            //    if (scope.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IMemberGroup>(e.SavedEntities)))
            //        e.Cancel = true;
            //}

            // v8
            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMemberGroup>(e.SavedEntities), this))
                e.Cancel = true;
        }

        void MemberGroupRepository_SavedMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
            // same as above!

            Saved.RaiseEvent(new SaveEventArgs<IMemberGroup>(e.SavedEntities, false), this);
        }

        #endregion

        public IEnumerable<IMemberGroup> GetAll()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _memberGroupRepository.GetMany();
            }
        }

        public IMemberGroup GetById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _memberGroupRepository.Get(id);
            }
        }

        public IMemberGroup GetByName(string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _memberGroupRepository.GetByName(name);
            }
        }

        public void Save(IMemberGroup memberGroup, bool raiseEvents = true)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IMemberGroup>(memberGroup);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _memberGroupRepository.Save(memberGroup);
                scope.Complete();

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs);
                }
            }
        }

        public void Delete(IMemberGroup memberGroup)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IMemberGroup>(memberGroup);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _memberGroupRepository.Delete(memberGroup);
                scope.Complete();
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(Deleted, this, deleteEventArgs);
            }
        }

        /// <summary>
        /// Occurs before Delete of a member group
        /// </summary>
        public static event TypedEventHandler<IMemberGroupService, DeleteEventArgs<IMemberGroup>> Deleting;

        /// <summary>
        /// Occurs after Delete of a member group
        /// </summary>
        public static event TypedEventHandler<IMemberGroupService, DeleteEventArgs<IMemberGroup>> Deleted;

        /// <summary>
        /// Occurs before Save of a member group
        /// </summary>
        /// <remarks>
        /// We need to proxy these events because the events need to take place at the repo level
        /// </remarks>
        public static event TypedEventHandler<IMemberGroupService, SaveEventArgs<IMemberGroup>> Saving;

        /// <summary>
        /// Occurs after Save of a member group
        /// </summary>
        /// <remarks>
        /// We need to proxy these events because the events need to take place at the repo level
        /// </remarks>
        public static event TypedEventHandler<IMemberGroupService, SaveEventArgs<IMemberGroup>> Saved;
    }
}
