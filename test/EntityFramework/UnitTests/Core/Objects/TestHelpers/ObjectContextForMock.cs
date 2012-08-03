﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;

    public class ObjectContextForMock : ObjectContext
    {
        private readonly EntityConnection _connection;

        internal ObjectContextForMock(EntityConnection connection)
        {
            _connection = connection;
        }

        public override DbConnection Connection
        {
            get { return _connection; }
        }

        public static ObjectContextForMock Create()
        {
            var providerFactoryMock = new Mock<DbProviderFactoryForMock>
                                          {
                                              CallBase = true
                                          };

            var entityConnectionMock = new Mock<EntityConnection>();
            entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactoryMock.Object);
            entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(default(DbConnection));
            var entityConnection = entityConnectionMock.Object;

            var objectContextMock = new Mock<ObjectContextForMock>(entityConnection)
                                        {
                                            CallBase = true
                                        };

            var metadataWorkspaceMock = new Mock<MetadataWorkspace>(MockBehavior.Strict);
            var metadataWorkspace = metadataWorkspaceMock.Object;

            var objectStateManagerMock = new Mock<ObjectStateManager>(metadataWorkspace);

            objectContextMock.Setup(m => m.ObjectStateManager).Returns(objectStateManagerMock.Object);

            var mockObjectQuery = MockHelper.CreateMockObjectQuery<DbDataRecord>(null);
            var mockObjectQueryProvider = new Mock<ObjectQueryProvider>(mockObjectQuery.Object);
            mockObjectQueryProvider.Setup(m => m.CreateQuery<DbDataRecord>(It.IsAny<Expression>()))
                .Returns(mockObjectQuery.Object);
            mockObjectQueryProvider.Setup(m => m.CreateQuery(It.IsAny<Expression>(), typeof(DbDataRecord)))
                .Returns(mockObjectQuery.Object);

            var fakeQueryable = new DbDataRecord[0].AsQueryable();
            mockObjectQuery.Setup(m => m.GetEnumeratorInternal()).Returns(fakeQueryable.GetEnumerator);
            mockObjectQuery.Setup(m => m.GetExpression()).Returns(() => fakeQueryable.Expression);
            mockObjectQuery.Setup(m => m.ObjectQueryProvider).Returns(() => mockObjectQueryProvider.Object);

            objectContextMock.Setup(m => m.CreateQuery<DbDataRecord>(It.IsAny<string>(), It.IsAny<ObjectParameter[]>())).Returns(
                () => mockObjectQuery.Object);

            return objectContextMock.Object;
        }
    }
}
