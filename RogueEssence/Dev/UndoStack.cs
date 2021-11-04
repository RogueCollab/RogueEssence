using RogueEssence.Ground;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace RogueEssence.Dev
{
    public abstract class Undoable
    {
        public virtual void Apply()
        {
            Redo();
        }

        public abstract void Undo();
        public abstract void Redo();
    }

    public abstract class SymmetricUndo : Undoable
    {
        public override void Undo()
        {
            Redo();
        }
    }

    public abstract class ReversibleUndo : Undoable
    {
        private bool reversed;
        public ReversibleUndo(bool reversed)
        {
            this.reversed = reversed;
        }

        public override void Undo()
        {
            if (reversed)
                Forward();
            else
                Backward();
        }
        public override void Redo()
        {
            if (reversed)
                Backward();
            else
                Forward();
        }

        public abstract void Forward();
        public abstract void Backward();
    }

    public abstract class StateUndo<T> : Undoable
    {
        private T past;
        private T result;

        public override void Apply()
        {
            T curState = GetState();
            //serialize, deserialize, assign
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                formatter.Serialize(stream, curState);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

                stream.Flush();
                stream.Position = 0;

#pragma warning disable SYSLIB0011 // Type or member is obsolete
                past = (T)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }
        }

        public override void Undo()
        {
            result = GetState();
            SetState(past);
        }

        public override void Redo()
        {
            SetState(result);
        }

        public abstract T GetState();
        public abstract void SetState(T state);
    }

    public class UndoStack
    {
        private Stack<Undoable> undos;
        private Stack<Undoable> redos;

        public UndoStack()
        {
            undos = new Stack<Undoable>();
            redos = new Stack<Undoable>();
        }

        public bool CanUndo
        {
            get { return undos.Count > 0; }
            set { }
        }

        public bool CanRedo
        {
            get { return redos.Count > 0; }
            set { }
        }

        public void Undo()
        {
            Undoable step = undos.Pop();
            step.Undo();
            redos.Push(step);
            DiagManager.Instance.LogInfo(String.Format("Undo {0}", step));
        }

        public void Redo()
        {
            Undoable step = redos.Pop();
            step.Redo();
            undos.Push(step);
            DiagManager.Instance.LogInfo(String.Format("Redo {0}", step));
        }

        public void Apply(Undoable step)
        {
            redos.Clear();
            step.Apply();
            undos.Push(step);
            DiagManager.Instance.LogInfo(String.Format("Apply {0}", step));
        }

        public void Clear()
        {
            undos.Clear();
            redos.Clear();
        }
    }
}
