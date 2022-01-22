using System.Collections.Generic;

namespace Lomont.Parser
{


    /// <summary>
    /// Process a stream of items, allowing a few basic operations:
    /// Can store position on a stack, allowing backing up in a 
    /// first-in, last-out order, useful for recursive descent parsing.
    /// </summary>
    /// <typeparam name="T">Type if item to stream</typeparam>
    /// <typeparam name="TPos">How to mark positions</typeparam>
    public abstract class ItemStream<T, TPos> where TPos : struct
    {


        /// <summary>
        /// get next, advance pos
        /// throw if none left
        /// </summary>
        /// <returns></returns>
        public abstract T Next();
        
        /// <summary>
        /// are there more?
        /// true if so
        /// </summary>
        /// <returns></returns>
        public abstract bool More();

        /// <summary>
        /// get current position
        /// </summary>
        /// <returns></returns>
        public TPos Pos() => curPos;

        /// <summary>
        /// advance position this many items, return new position
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public TPos Consume(int size = 1)
        {
            for (var i = 0; i < size; ++i)
                Next();
            return curPos;
        }

        /// <summary>
        /// look at next item
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            SavePosition();
            var t = Next();
            RestorePosition();
            return t;
        }

        /// <summary>
        /// Discard the last saved position,
        /// i.e., commit the new position
        /// </summary>
        public void DiscardPosition() => posStack.Pop();

        /// <summary>
        /// save current position on a stack
        /// </summary>
        public void SavePosition() => posStack.Push(curPos);

        /// <summary>
        /// Restore the last saved position
        /// </summary>
        public void RestorePosition() => curPos = posStack.Pop();

        protected TPos curPos;

        /// <summary>
        /// Stack of saves positions
        /// </summary>
        Stack<TPos> posStack = new Stack<TPos>();

    }

}
