












//AUTOGENERATED, DO NOTMODIFY.
//Do not edit this file directly.

#pragma warning disable 1591
// ReSharper disable CheckNamespace

using System;
using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Model;
using RethinkDb.Driver.Proto;
using System.Collections;
using System.Collections.Generic;


namespace RethinkDb.Driver.Ast {

    public partial class MakeObj : ReqlExpr {

    
    
    
        public MakeObj(Object arg) : this(new Arguments(arg), null) {

        }
        public MakeObj(OptArgs opts) : this(new Arguments(), opts) {
        }
        public MakeObj(Arguments args) : this(args, null) {
        }
        public MakeObj(Arguments args, OptArgs optargs) : 
               this(TermType.MAKE_OBJ, args, optargs) {
        }
        protected MakeObj(TermType termType, Arguments args, OptArgs optargs)
              : base(termType, args, optargs){

        }


    



    


    












    
        internal static MakeObj fromMap(Dictionary<string, ReqlAst> map){
            return new MakeObj(OptArgs.fromMap(map));
        }
        public static MakeObj FromMap(Dictionary<string, ReqlAst> map){
            return fromMap(map);
        }



    
    }
}
