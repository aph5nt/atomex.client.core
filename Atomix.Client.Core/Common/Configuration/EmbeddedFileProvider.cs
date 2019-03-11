﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Atomix.Common.Configuration
{
    public class EmbeddedFileProvider : IFileProvider
    {
        private readonly Assembly _assembly;

        public EmbeddedFileProvider(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        public IFileInfo GetFileInfo(string subPath)
        {
            var fullFileName = $"{_assembly.GetName().Name}.{subPath}";

            var isFileEmbedded = _assembly.GetManifestResourceNames().Contains(fullFileName);

            return isFileEmbedded
                ? new EmbeddedFileInfo(subPath, _assembly.GetManifestResourceStream(fullFileName))
                : (IFileInfo)new NotFoundFileInfo(subPath);
        }

        public IDirectoryContents GetDirectoryContents(string subPath)
        {
            throw new NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }
    }
}