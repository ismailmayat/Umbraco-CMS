﻿using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class AuditRepositoryTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Add_Audit_Entry()
        {
            var sp = TestObjects.GetScopeProvider(Logger);
            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor) sp, CacheHelper, Logger);
                repo.Save(new AuditItem(-1, "This is a System audit trail", AuditType.System, 0));

                var dtos = scope.Database.Fetch<LogDto>("WHERE id > -1");

                Assert.That(dtos.Any(), Is.True);
                Assert.That(dtos.First().Comment, Is.EqualTo("This is a System audit trail"));
            }
        }

        [Test]
        public void Get_Paged_Items()
        {
            var sp = TestObjects.GetScopeProvider(Logger);
            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor) sp, CacheHelper, Logger);

                for (var i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, $"Content {i} created", AuditType.New, 0));
                    repo.Save(new AuditItem(i, $"Content {i} published", AuditType.Publish, 0));
                }

                scope.Complete();
            }

            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor) sp, CacheHelper, Logger);

                var page = repo.GetPagedResultsByQuery(sp.SqlContext.Query<IAuditItem>(), 0, 10, out var total, Direction.Descending, null, null);

                Assert.AreEqual(10, page.Count());
                Assert.AreEqual(200, total);
            }
        }

        [Test]
        public void Get_Paged_Items_With_AuditType_Filter()
        {
            var sp = TestObjects.GetScopeProvider(Logger);
            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor) sp, CacheHelper, Logger);

                for (var i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, $"Content {i} created", AuditType.New, 0));
                    repo.Save(new AuditItem(i, $"Content {i} published", AuditType.Publish, 0));
                }

                scope.Complete();
            }

            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor) sp, CacheHelper, Logger);

                var page = repo.GetPagedResultsByQuery(sp.SqlContext.Query<IAuditItem>(), 0, 9, out var total, Direction.Descending,
                        new[] {AuditType.Publish}, null)
                    .ToArray();

                Assert.AreEqual(9, page.Length);
                Assert.IsTrue(page.All(x => x.AuditType == AuditType.Publish));
                Assert.AreEqual(100, total);
            }
        }

        [Test]
        public void Get_Paged_Items_With_Custom_Filter()
        {
            var sp = TestObjects.GetScopeProvider(Logger);
            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor) sp, CacheHelper, Logger);

                for (var i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, "Content created", AuditType.New, 0));
                    repo.Save(new AuditItem(i, "Content published", AuditType.Publish, 0));
                }

                scope.Complete();
            }

            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor) sp, CacheHelper, Logger);

                var page = repo.GetPagedResultsByQuery(sp.SqlContext.Query<IAuditItem>(), 0, 8, out var total, Direction.Descending,
                        null, sp.SqlContext.Query<IAuditItem>().Where(item => item.Comment == "Content created"))
                    .ToArray();

                Assert.AreEqual(8, page.Length);
                Assert.IsTrue(page.All(x => x.Comment == "Content created"));
                Assert.AreEqual(100, total);
            }
        }
    }
}
