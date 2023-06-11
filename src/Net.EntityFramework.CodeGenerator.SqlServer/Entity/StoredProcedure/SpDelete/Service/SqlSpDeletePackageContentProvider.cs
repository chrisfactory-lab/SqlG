﻿using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Net.EntityFramework.CodeGenerator.Core;

namespace Net.EntityFramework.CodeGenerator.SqlServer
{
    internal class SqlSpDeletePackageContentProvider : IIntentContentProvider
    {
        private readonly IDbContextModelContext _context;
        private readonly IEntityTypeTable _entityTable;
        private readonly IDataProjectFileInfoFactory _fileInfoFactory;
        private readonly ISpDeleteParametersProvider _parametersProvider;
        private readonly IStoredProcedureSchemaProvider _schemaProvider;
        private readonly IStoredProcedureNameProvider _spNameProvider;
        public SqlSpDeletePackageContentProvider(
            IDbContextModelContext context,
            IMutableEntityType mutableEntity,
            IDataProjectFileInfoFactory fiFoctory,
            ISpDeleteParametersProvider parametersProvider,
            IStoredProcedureSchemaProvider schemaProvider,
            IStoredProcedureNameProvider spNameProvider)
        {
            _context = context;
            _entityTable = context.GetEntity(mutableEntity);
            _fileInfoFactory = fiFoctory;
            _parametersProvider = parametersProvider;
            _schemaProvider = schemaProvider;
            _spNameProvider = spNameProvider;
        }


        public IEnumerable<IContent> Get()
        {
            var dbProjOptions = _context.DataProjectTargetInfos;
            var rootPath = dbProjOptions.RootPath;
            var pattern = dbProjOptions.StoredProceduresPatternPath;
            var spSchema = _schemaProvider.Get();
            var spName = _spNameProvider.Get();
            var targetTableFullName = _entityTable.TableFullName;
            var tableName = _entityTable.Table.Name;
            var output = _entityTable.AllColumns;
            var parameters = _parametersProvider.GetParameters();
            var fileName = spName;
            var spOptions = new StoredProcedureOptions(spSchema, spName);
            var sp = new StoredProcedureGenerator(spOptions, parameters, new DeleteGenerator(targetTableFullName, output, parameters));
            var spCode = sp.Build();

            var fi = _fileInfoFactory.CreateFileInfo(rootPath, fileName, pattern, spSchema, tableName, null, spName);
            yield return new ContentFile(fi, new CommandTextSegment(spCode));

        }
    }
}
