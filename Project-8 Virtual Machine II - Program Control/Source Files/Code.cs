using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System;

namespace Project8
{
    public class Code
    {
        private const int SP_BASE = 256;

        private const string SP = "SP";

        private const string TEMP_BASE = "R5";

        private const int TRUE = -1;

        private const int FALSE = 0;

        private const string SEG = "${SEG}$";

        private const string VAR = "${VAR}$";

        private const string IDX = "${IDX}$";

        private const string CMD = "${CMD}$";

        private const string LBL = "${LBL}$";

        private const string FUN = "${FUN}$";

        private static readonly Dictionary<string, string> segEnv =
            new Dictionary<string, string>()
        {
            { "local", "LCL" },
            { "argument", "ARG" },
            { "this", "THIS" },
            { "that", "THAT" }
        };

        private static readonly string[] orderedStoreSegEnv = new string[]
        {
            "LCL", "ARG", "THIS", "THAT"
        };

        private static readonly Dictionary<string, string> arithCmds =
            new Dictionary<string, string>()
        {
            { "add", "M = M + D" },
            { "sub", "M = M - D" },
            { "neg", "M = -M" },
            { "eq", "JEQ" },
            { "gt", "JGT" },
            { "lt", "JLT" },
            { "and", "M = M & D" },
            { "or", "M = M | D" },
            { "not", "M = !M" }
        };

        /******************* init ********************/
        private static readonly string INIT_TEMPLATE =
            $"@{SP_BASE}" + Environment.NewLine +
            "D = A" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            $"{CMD}" + Environment.NewLine +
            $"(END{IDX})" + Environment.NewLine +
            $"@END{IDX}" + Environment.NewLine +
            "0; JMP" + Environment.NewLine;

        /******************** push ********************/
        private static readonly string PUSH_TEMPLATE_END =
            $"@{SP}" + Environment.NewLine +
            "A = M" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "M = M + 1" + Environment.NewLine;

        private static readonly string PUSH_TEMPLATE =
            $"@{SEG}" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            $"@{IDX}" + Environment.NewLine +
            "A = D + A" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            PUSH_TEMPLATE_END;

        private static readonly string PUSH_TEMP_TEMPLATE =
            $"@{TEMP_BASE}" + Environment.NewLine +
            "D = A" + Environment.NewLine +
            $"@{IDX}" + Environment.NewLine +
            "A = D + A" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            PUSH_TEMPLATE_END;

        private static readonly string PUSH_CONST_TEMPLATE =
            $"@{IDX}" + Environment.NewLine +
            "D = A" + Environment.NewLine +
            PUSH_TEMPLATE_END;

        private static readonly string PUSH_VAR_TEMPLATE =
            $"@{VAR}" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            PUSH_TEMPLATE_END;

        /******************** pop ********************/
        private static readonly string POP_TEMPLATE_END =
            "@R13" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "AM = M - 1" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            "@R13" + Environment.NewLine +
            "A = M" + Environment.NewLine +
            "M = D" + Environment.NewLine;

        private static readonly string POP_TEMPLATE =
            $"@{SEG}" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            $"@{IDX}" + Environment.NewLine +
            "D = D + A" + Environment.NewLine +
            POP_TEMPLATE_END;

        private static readonly string POP_TEMP_TEMPLATE =
            $"@{TEMP_BASE}" + Environment.NewLine +
            "D = A" + Environment.NewLine +
            $"@{IDX}" + Environment.NewLine +
            "D = D + A" + Environment.NewLine +
            POP_TEMPLATE_END;

        private static readonly string POP_VAR_TEMPLATE =
            $"@{VAR}" + Environment.NewLine +
            "D = A" + Environment.NewLine +
            POP_TEMPLATE_END;

        /******************** not ********************/
        /******************** neg ********************/
        private static readonly string ONE_ARG_OP_TEMPLATE =
            $"@{SP}" + Environment.NewLine +
            "AM = M - 1" + Environment.NewLine +
            $"{CMD}" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "M = M + 1" + Environment.NewLine;

        /******************** add ********************/
        /******************** sub ********************/
        /******************** and ********************/
        /******************** or *********************/
        private static readonly string TWO_ARG_OP_TEMPLATE =
            $"@{SP}" + Environment.NewLine +
            "AM = M - 1" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            "A = A - 1" + Environment.NewLine +
            $"{CMD}" + Environment.NewLine;

        /******************** eq *********************/
        /******************** gt *********************/
        /******************** lt *********************/
        private static readonly string CMP_TEMPLATE =
            $"@{SP}" + Environment.NewLine +
            "AM = M - 1" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            "A = A - 1" + Environment.NewLine +
            "D = M - D" + Environment.NewLine +
            $"@TRUE{IDX}" + Environment.NewLine +
            $"D; {CMD}" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "AM = M - 1" + Environment.NewLine +
            $"M = {FALSE}" + Environment.NewLine +
            $"@CONTINUE{IDX}" + Environment.NewLine +
            "0; JMP" + Environment.NewLine +
            $"(TRUE{IDX})" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "AM = M - 1" + Environment.NewLine +
            $"M = {TRUE}" + Environment.NewLine +
            $"(CONTINUE{IDX})" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "M = M + 1" + Environment.NewLine;

        /******************* goto ********************/
        private static readonly string GOTO_TEMPLATE =
            $"@{LBL}" + Environment.NewLine +
            "0; JMP" + Environment.NewLine;

        /******************** if *********************/
        private static readonly string IF_TEMPLATE =
            $"@{SP}" + Environment.NewLine +
            "AM = M - 1" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            $"@{LBL}" + Environment.NewLine +
            "D; JNE" + Environment.NewLine;

        /******************* call ********************/
        private static readonly string CALL_TEMPLATE =
            // save segment pointers...
            $"@{IDX}" + Environment.NewLine +
            "D = A" + Environment.NewLine +
            $"@{segEnv.Count + 1}" + Environment.NewLine +
            "D = A + D" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "D = M - D" + Environment.NewLine +
            $"@{segEnv["argument"]}" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            $"@{segEnv["local"]}" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            $"@{FUN}" + Environment.NewLine +
            "0; JMP" + Environment.NewLine +
            $"{LBL}" + Environment.NewLine;

        /****************** return *******************/
        private static readonly string RESTORE_SEG_TEMPLATE =
            "@R13" + Environment.NewLine +
            "D = M - 1" + Environment.NewLine +
            "AM = D" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            $"@{VAR}" + Environment.NewLine +
            "M = D" + Environment.NewLine;

        private static readonly string RET_TEMPLATE =
            $"@{segEnv["local"]}" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            "@R13" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            $"@{segEnv.Count + 1}" + Environment.NewLine +
            "A = D - A" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            "@R14" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "AM = M - 1" + Environment.NewLine +
            "D = M" + Environment.NewLine +
            $"@{segEnv["argument"]}" + Environment.NewLine +
            "A = M" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            $"@{segEnv["argument"]}" + Environment.NewLine +
            "D = M + 1" + Environment.NewLine +
            $"@{SP}" + Environment.NewLine +
            "M = D" + Environment.NewLine +
            // restore segment pointers...
            $"{CMD}" + Environment.NewLine +
            "@R14" + Environment.NewLine +
            "A = M" + Environment.NewLine +
            "0; JMP" + Environment.NewLine;

        private UniqueLabelIndex labelIdx = new UniqueLabelIndex();

        private FuncRetLabelIndex RetAddrIdx = new FuncRetLabelIndex();

        private string fileName = null;

        public Code(string fileName)
        {
            SetFileNamePrefix(fileName);
        }

        public void SetFileNamePrefix(string fileName)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(fileName));

            this.fileName = fileName;
        }

        public string Init()
        {
            var values = new Dictionary<string, string>()
            {
                { CMD, Call("Sys.init", 0) },
                { IDX, labelIdx.Next().ToString() }
            };

            return Replace(INIT_TEMPLATE, values);
        }

        public string Arithmetic(string command)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(command));
            Debug.Assert(arithCmds.ContainsKey(command));

            var values = new Dictionary<string, string>()
            {
                { CMD, arithCmds[command] }
            };

            switch (command)
            {
                case "add":
                case "sub":
                case "and":
                case "or":
                {
                    return Replace(TWO_ARG_OP_TEMPLATE, values);
                }
                case "not":
                case "neg":
                {
                    return Replace(ONE_ARG_OP_TEMPLATE, values);
                }
                case "eq":
                case "gt":
                case "lt":
                {
                    values.Add(IDX, labelIdx.Next().ToString());
                    return Replace(CMP_TEMPLATE, values);
                }
                default:
                {
                    Debug.Assert(false);
                    return null;
                }
            }
        }

        public string Push(string segment, int index)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(segment));

            switch (segment)
            {
                case "constant":
                {
                    return PushConstant(index.ToString());
                }
                case "temp":
                {
                    return Replace(PUSH_TEMP_TEMPLATE,
                        new Dictionary<string, string>(){{ IDX, index.ToString() }});
                }
                case "static":
                {
                    return PushVariable(StaticVarName(index));
                }
                case "pointer":
                {
                    if (index == 0)
                    {
                        return PushVariable(segEnv["this"]);
                    }
                    else if (index == 1)
                    {
                        return PushVariable(segEnv["that"]);
                    }
                    else
                    {
                        throw new ArgumentException(
                            " [!] The index is invalid.");
                    }
                }
                default:
                {
                    if (!segEnv.ContainsKey(segment))
                    {
                        throw new KeyNotFoundException(
                            " [!] The segment is not in the set.");
                    }

                    var values = new Dictionary<string, string>()
                    {
                        { IDX, index.ToString() },
                        { SEG, segEnv[segment] }
                    };

                    return Replace(PUSH_TEMPLATE, values);
                }
            }
        }

        public string Pop(string segment, int index)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(segment));
            Debug.Assert(segment != "constant");

            switch (segment)
            {
                case "temp":
                {
                    return Replace(POP_TEMP_TEMPLATE,
                        new Dictionary<string, string>(){{ IDX, index.ToString() }});
                }
                case "static":
                {
                    return PopVariable(StaticVarName(index));
                }
                case "pointer":
                {
                    if (index == 0)
                    {
                        return PopVariable(segEnv["this"]);
                    }
                    else if (index == 1)
                    {
                        return PopVariable(segEnv["that"]);
                    }
                    else
                    {
                        throw new ArgumentException(
                            " [!] The index is invalid.");
                    }
                }
                default:
                {
                    if (!segEnv.ContainsKey(segment))
                    {
                        throw new KeyNotFoundException(
                            " [!] The segment is not in the set.");
                    }

                    var values = new Dictionary<string, string>()
                    {
                        { IDX, index.ToString() },
                        { SEG, segEnv[segment] }
                    };

                    return Replace(POP_TEMPLATE, values);
                }
            }
        }

        public string Goto(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            return Replace(GOTO_TEMPLATE,
                new Dictionary<string, string>(){{ LBL, LabelName(label) }});
        }

        public string Label(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            return $"({LabelName(label)})";
        }

        public string If(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            return Replace(IF_TEMPLATE,
                new Dictionary<string, string>(){{ LBL, LabelName(label) }});
        }

        public string Function(string function, int vars)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(function));

            StringBuilder sb = new StringBuilder($"({function})" + Environment.NewLine);

            for (int i = 0; i != vars; ++i)
            {
                sb.Append(PushConstant("0"));
            }

            return sb.ToString();
        }

        public string Call(string function, int args)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(function));

            string retLabel = RetAddrName(function);

            StringBuilder sb = new StringBuilder();
            sb.Append(PushConstant(retLabel));

            foreach (string seg in orderedStoreSegEnv)
            {
                sb.Append(PushVariable(seg));
            }

            var values = new Dictionary<string, string>()
            {
                { IDX, args.ToString() },
                { FUN, function },
                { LBL, $"({retLabel})" }
            };

            sb.Append(Replace(CALL_TEMPLATE, values));

            return sb.ToString();
        }

        public string Return()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = orderedStoreSegEnv.Length - 1; i >= 0; --i)
            {
                sb.Append(Replace(RESTORE_SEG_TEMPLATE,
                    new Dictionary<string, string>(){{ VAR, orderedStoreSegEnv[i] }}));
            }

            return Replace(RET_TEMPLATE,
                new Dictionary<string, string>(){{ CMD, sb.ToString() }});
        }

        private static string PushVariable(string name)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));

            return Replace(PUSH_VAR_TEMPLATE,
                new Dictionary<string, string>(){{ VAR, name }});
        }

        private static string PushConstant(string name)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));

            return Replace(PUSH_CONST_TEMPLATE,
                new Dictionary<string, string>(){{ IDX, name }});
        }

        private static string PopVariable(string name)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(name));

            return Replace(POP_VAR_TEMPLATE,
                new Dictionary<string, string>(){{ VAR, name }});
        }

        private static string Replace(string template,
            Dictionary<string, string> values)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(template));
            Debug.Assert(values != null);

            foreach (var pair in values)
            {
                template = template.Replace(pair.Key, pair.Value);
            }

            return template;
        }

        private string StaticVarName(int index)
        {
            return $"{fileName}.{index}";
        }

        private string LabelName(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            return $"{fileName}${label}";
        }

        private string RetAddrName(string function)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(function));

            int idx = RetAddrIdx.Next(function);
            return $"{function}$ret.{idx}";
        }
    }
}