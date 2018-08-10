﻿using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Tarzan.Nfx.Model;
using System.Diagnostics;
using System.Linq;

namespace Tarzan.Nfx.Model.Cassandra
{
    public class ModelMapping : Mappings
    {
        public static void AutoRegister(MappingConfiguration global)
        {
            var definedTypes = Assembly.GetExecutingAssembly().DefinedTypes;
            foreach(var definedType in definedTypes)
            {
                //var prop = definedType.GetProperty("Mapping");
                var requiredType = typeof(Map<>).GetTypeInfo().MakeGenericType(definedType);
                var prop = definedType.DeclaredProperties.FirstOrDefault(pi => requiredType.Equals(pi.PropertyType.GetTypeInfo()));
                if (prop != null) 
                { 
                    var val = prop.GetValue(null) as ITypeDefinition;                    
                    Debug.Write($"Mapping '{definedType.FullName}' class to Cassandra table 'val.TableName'.");
                    global.Define(val);
                }
            }
        }
    }
}
