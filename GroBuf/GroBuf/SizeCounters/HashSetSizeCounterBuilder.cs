﻿using System;
using System.Collections.Generic;
using System.Reflection;

using GrEmit;

namespace GroBuf.SizeCounters
{
    internal class HashSetSizeCounterBuilder : SizeCounterBuilderBase
    {
        public HashSetSizeCounterBuilder(Type type)
            : base(type)
        {
            if(!(Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(HashSet<>)))
                throw new InvalidOperationException("HashSet expected but was '" + Type + "'");
            elementType = Type.GetGenericArguments()[0];
        }

        protected override void BuildConstantsInternal(SizeCounterConstantsBuilderContext context)
        {
            context.BuildConstants(elementType);
        }

        protected override bool CheckEmpty(SizeCounterMethodBuilderContext context, GroboIL.Label notEmptyLabel)
        {
            var emptyLabel = context.Il.DefineLabel("empty");
            context.LoadObj(); // stack: [obj]
            context.Il.Brfalse(emptyLabel); // if(obj == null) goto empty;
            context.LoadObj(); // stack: [obj]
            context.Il.Ldfld(Type.GetField("m_count", BindingFlags.Instance | BindingFlags.NonPublic)); // stack: [obj.Count]
            context.Il.Brtrue(notEmptyLabel); // if(obj.Count != 0) goto notEmpty;
            context.Il.MarkLabel(emptyLabel);
            return true;
        }

        protected override void CountSizeNotEmpty(SizeCounterMethodBuilderContext context)
        {
            var il = context.Il;
            il.Ldc_I4(9); // stack: [9 = size] 9 = type code + data length + hashset count
            context.LoadObj(); // stack: [size, obj]
            var count = il.DeclareLocal(typeof(int));
            il.Ldfld(Type.GetField("m_count", BindingFlags.Instance | BindingFlags.NonPublic));
            il.Stloc(count);
            context.LoadObj(); // stack: [size, obj]
            var slotType = Type.GetNestedType("Slot", BindingFlags.NonPublic).MakeGenericType(Type.GetGenericArguments());
            var slots = il.DeclareLocal(slotType.MakeArrayType());
            il.Ldfld(Type.GetField("m_slots", BindingFlags.Instance | BindingFlags.NonPublic));
            il.Stloc(slots);

            var i = il.DeclareLocal(typeof(int));
            il.Ldc_I4(0); // stack: [9, 0]
            il.Stloc(i); // i = 0; stack: [9]
            var cycleStartLabel = il.DefineLabel("cycleStart");
            il.MarkLabel(cycleStartLabel);
            il.Ldloc(slots); // stack: [size, slots]
            il.Ldloc(i); // stack: [size, slots, i]
            il.Ldelema(slotType); // stack: [size, &slots[i]]
            il.Dup(); // stack: [size, &slots[i], &slots[i]]
            var slot = il.DeclareLocal(slotType.MakeByRefType());
            il.Stloc(slot); // slot = &slots[i]; stack: [size, slot]
            il.Ldfld(slotType.GetField("hashCode", BindingFlags.Instance | BindingFlags.NonPublic)); // stack: [size, slot.hashCode]
            il.Ldc_I4(0); // stack: [size, slot.hashCode, 0]
            var nextLabel = il.DefineLabel("next");
            il.Blt(typeof(int), nextLabel); // if(slot.hashCode < 0) goto next; stack: [size]
            il.Ldloc(slot); // stack: [size, slot]
            il.Ldfld(slotType.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic)); // stack: [size, slot.value]
            il.Ldc_I4(1); // stack: [size, slot.value, true]
            context.CallSizeCounter(elementType); // stack: [size, writer(slot.value, true) = valueSize]
            il.Add(); // stack: [size + valueSize]

            il.MarkLabel(nextLabel);
            il.Ldloc(count); // stack: [size, count]
            il.Ldloc(i); // stack: [size, count, i]
            il.Ldc_I4(1); // stack: [size, count, i, 1]
            il.Add(); // stack: [size, count, i + 1]
            il.Dup(); // stack: [size, count, i + 1, i + 1]
            il.Stloc(i); // i = i + 1; stack: [size, count, i]
            il.Bgt(typeof(int), cycleStartLabel); // if(count > i) goto cycleStart; stack: [size]
        }

        private readonly Type elementType;
    }
}