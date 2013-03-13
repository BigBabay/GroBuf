using System;
using System.Reflection;

using GrEmit;

namespace GroBuf.SizeCounters
{
    internal abstract class SizeCounterBuilderBase : ISizeCounterBuilder
    {
        protected SizeCounterBuilderBase(Type type)
        {
            Type = type;
        }

        public MethodInfo BuildSizeCounter(SizeCounterTypeBuilderContext sizeCounterTypeBuilderContext)
        {
            var typeBuilder = sizeCounterTypeBuilderContext.TypeBuilder;

            var method = typeBuilder.DefineMethod("Count_" + Type.Name + "_" + Guid.NewGuid(), MethodAttributes.Public | MethodAttributes.Static, typeof(int),
                                                  new[]
                                                      {
                                                          Type, typeof(bool)
                                                      });
            sizeCounterTypeBuilderContext.SetCounter(Type, method);
            var il = new GroboIL(method);
            var context = new SizeCounterMethodBuilderContext(sizeCounterTypeBuilderContext, il);

            var notEmptyLabel = il.DefineLabel("notEmpty");
            if(CheckEmpty(context, notEmptyLabel)) // Check if obj is empty
                context.ReturnForNull(); // return for null
            il.MarkLabel(notEmptyLabel); // Now we know that obj is not empty
            CountSizeNotEmpty(context); // Count size
            il.Ret();
            return method;
        }

        protected abstract void CountSizeNotEmpty(SizeCounterMethodBuilderContext context);

        /// <summary>
        /// Checks whether <c>obj</c> is empty
        /// </summary>
        /// <param name="context">Current context</param>
        /// <param name="notEmptyLabel">Label where to go if <c>obj</c> is not empty</param>
        /// <returns>true if <c>obj</c> can be empty</returns>
        protected virtual bool CheckEmpty(SizeCounterMethodBuilderContext context, GroboIL.Label notEmptyLabel)
        {
            if(!Type.IsClass) return false;
            context.LoadObj(); // stack: [obj]
            context.Il.Brtrue(notEmptyLabel); // if(obj != null) goto notEmpty;
            return true;
        }

        protected Type Type { get; private set; }
    }
}