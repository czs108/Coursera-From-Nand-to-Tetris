using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace Project11
{
    public class SymbolTable
    {
        private class Identifier
        {
            public string name;
            public string type;
            public IdentifierKind kind;
            public int index;
        }

        private static Dictionary<string, Identifier> staticVars =
            new Dictionary<string, Identifier>();

        private Dictionary<string, Identifier> fieldVars =
            new Dictionary<string, Identifier>();

        private Dictionary<string, Identifier> localArgs =
            new Dictionary<string, Identifier>();

        private Dictionary<string, Identifier> localVars =
            new Dictionary<string, Identifier>();

        public void StartSubroutine()
        {
            localVars.Clear();
            localArgs.Clear();
        }

        public void Define(string name, string type, IdentifierKind kind)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));
            Debug.Assert(!String.IsNullOrWhiteSpace(type));

            Dictionary<string, Identifier> dest = null;
            switch (kind)
            {
                case IdentifierKind.Static:
                {
                    dest = staticVars;
                    break;
                }
                case IdentifierKind.Field:
                {
                    dest = fieldVars;
                    break;
                }
                case IdentifierKind.Arg:
                {
                    dest = localArgs;
                    break;
                }
                case IdentifierKind.Var:
                {
                    dest = localVars;
                    break;
                }
                default:
                {
                    Debug.Assert(false);
                    break;
                }
            }

            Identifier id = new Identifier()
            {
                name = name,
                type = type,
                kind = kind,
                index = dest.Count
            };

            dest.Add(name, id);
        }

        public int VarCount(IdentifierKind kind)
        {
            switch (kind)
            {
                case IdentifierKind.Static:
                    return staticVars.Count;
                case IdentifierKind.Field:
                    return fieldVars.Count;
                case IdentifierKind.Arg:
                    return localArgs.Count;
                case IdentifierKind.Var:
                    return localVars.Count;
                default:
                {
                    Debug.Assert(false);
                    return 0;
                }
            }
        }

        public bool Contain(string name) => Search(name) != null;

        public IdentifierKind KindOf(string name)
        {
            Debug.Assert(Contain(name));

            return Search(name).kind;
        }

        public string TypeOf(string name)
        {
            Debug.Assert(Contain(name));

            return Search(name).type;
        }

        public int IndexOf(string name)
        {
            Debug.Assert(Contain(name));

            return Search(name).index;
        }

        private Identifier Search(string name)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));

            if (localVars.ContainsKey(name))
            {
                return localVars[name];
            }
            else if (localArgs.ContainsKey(name))
            {
                return localArgs[name];
            }
            else if (fieldVars.ContainsKey(name))
            {
                return fieldVars[name];
            }
            else if (staticVars.ContainsKey(name))
            {
                return staticVars[name];
            }
            else
            {
                return null;
            }
        }
    }
}