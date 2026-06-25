using System;

using System.Collections.Generic;

using System.Reflection;

using HarmonyLib;



namespace PocketRouletteFikaFix

{

    internal static class ModAssemblyTypes

    {

        private static readonly Dictionary<string, Type> TypeCache =

            new Dictionary<string, Type>(StringComparer.Ordinal);



        internal static Type FindType(string fullName)

        {

            if (string.IsNullOrEmpty(fullName))

            {

                return null;

            }



            if (TypeCache.TryGetValue(fullName, out var cached))

            {

                return cached;

            }



            var direct = AccessTools.TypeByName(fullName);

            if (direct != null)

            {

                TypeCache[fullName] = direct;

                return direct;

            }



            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())

            {

                try

                {

                    var type = assembly.GetType(fullName, false);

                    if (type != null)

                    {

                        TypeCache[fullName] = type;

                        return type;

                    }

                }

                catch

                {

                    // ignored

                }

            }



            TypeCache[fullName] = null;

            return null;

        }



        internal static MethodInfo FindMethod(Type type, string name, Type[] parameters = null)

        {

            if (type == null)

            {

                return null;

            }



            return parameters == null

                ? AccessTools.Method(type, name)

                : AccessTools.Method(type, name, parameters);

        }

    }

}


