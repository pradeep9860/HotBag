﻿using HotBag.EntityBase;
using HotBag.Web.Core.Web.Attributes;
using HotBag.Web.GenericBase;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace HotBag.Web.Core.Web
{
    public class RemoteControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var remoteCode = new HttpClient().GetStringAsync("https://gist.githubusercontent.com/filipw/9311ce866edafde74cf539cbd01235c9/raw/6a500659a1c5d23d9cfce95d6b09da28e06c62da/types.txt").GetAwaiter().GetResult();
            if (remoteCode != null)
            {
                var compilation = CSharpCompilation.Create("DynamicAssembly",
                    new[] { CSharpSyntaxTree.ParseText(remoteCode) },
                    new[] {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(RemoteControllerFeatureProvider).Assembly.Location)
                    },
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var ms = new MemoryStream())
                {
                    var emitResult = compilation.Emit(ms);

                    if (!emitResult.Success)
                    {
                        // handle, log errors etc
                        Debug.WriteLine("Compilation failed!");
                        return;
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(ms.ToArray());
                    var candidates = assembly.GetExportedTypes();

                    foreach (var candidate in candidates)
                    {
                        feature.Controllers.Add(
                            typeof(GenericBaseController<,,>).MakeGenericType(candidate).GetTypeInfo()
                        );
                    }
                }
            }
        }
    }

    [GeneratedController("api/book")]
    [GeneratedControllerDto(typeof(BookDto), typeof(Guid))]
    public class Book : EntityBase<Guid>
    {  
        public string Title { get; set; }

        public string Author { get; set; }
    }

    public class BookDto : EntityBaseDto<Guid>
    {
        public string Title { get; set; }

        public string Author { get; set; }
    }

    [GeneratedController("api/v1/album")]
    [GeneratedControllerDto(typeof(AlbumDto), typeof(Guid))]
    public class Album : EntityBase<Guid>
    { 
        public string Title { get; set; }

        public string Artist { get; set; }
    }

    public class AlbumDto : EntityBaseDto<Guid>
    {
        public string Title { get; set; }

        public string Artist { get; set; }
    }
}
