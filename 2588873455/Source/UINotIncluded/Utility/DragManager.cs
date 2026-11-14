using System;
using System.Collections.Generic;
using UnityEngine;

namespace UINotIncluded
{
    public static class DragMemory
    {
        public static DragElement? _dragged;
        public static DragElement? hoveringOver;
        public static bool Dragging => _dragged != null;
        public static bool Hovering => hoveringOver != null;

        public static DragElement? Dragged => _dragged;
    }

    public class DragManager<T>
    {
        private readonly Dictionary<string, List<T>> _managed_dragable_lists = new Dictionary<string, List<T>>();
        private readonly Func<T, Action> _OnClick;
        public Func<T, string> GetLabel;
        public Action OnUpdate;

        public DragManager(Action OnUpdate, Func<T, string> GetLabel)
        {
            this.OnUpdate = OnUpdate;
            this.GetLabel = GetLabel;
        }

        public DragManager(Action OnUpdate, Func<T, string> GetLabel, Func<T, Action> OnClick)
        {
            this.OnUpdate = OnUpdate;
            this.GetLabel = GetLabel;
            _OnClick = OnClick;
        }

        public void ManageList(string name, List<T> list)
        {
            _managed_dragable_lists[name] = list;
        }

        public void Update()
        {
            DrawGhost();
            if (DraggStops())
                if (DragMemory.Dragging)
                {
                    MoveDragged();
                    DragMemory._dragged = null;
                    OnUpdate();
                }
        }

        public void DraggStart(DragElement element)
        {
            DragMemory._dragged = element;
        }

        public bool DraggStops()
        {
            return Event.current.rawType == EventType.MouseUp;
        }

        public bool HasOnClick(DragElement element)
        {
            if (_OnClick == null) return false;
            return _OnClick(_managed_dragable_lists[element.listname][element.pos]) != null;
        }

        public void OnClick(DragElement element)
        {
            if (_OnClick == null) return;
            _OnClick(_managed_dragable_lists[element.listname][element.pos])?.Invoke();
        }

        public void DrawGhost()
        {
            if (Event.current.type != EventType.Repaint || !DragMemory.Dragging ||
                !_managed_dragable_lists.ContainsKey(DragMemory.Dragged?.listname)) return;
            var offset = (float)Math.Floor((float)DragMemory.Dragged?.size.y / 2);
            var pos = Event.current.mousePosition - new Vector2(offset, offset);
            CustomButtons.DraggableButtonGhost(new Rect(pos, (Vector2)DragMemory.Dragged?.size),
                GetLabel(_managed_dragable_lists[DragMemory.Dragged?.listname][(int)DragMemory.Dragged?.pos]));
        }

        public void MoveDragged()
        {
            if (!DragMemory.Dragging || !DragMemory.Hovering) return;
            var origin = _managed_dragable_lists[DragMemory._dragged?.listname];
            var destination = _managed_dragable_lists[DragMemory.hoveringOver?.listname];

            var sameList = DragMemory._dragged?.listname == DragMemory.hoveringOver?.listname;
            var removePos = sameList
                ? (int)DragMemory.hoveringOver?.pos > (int)DragMemory._dragged?.pos
                    ? (int)DragMemory._dragged?.pos
                    : (int)DragMemory._dragged?.pos + 1
                : (int)DragMemory._dragged?.pos;

            var temp = origin[(int)DragMemory._dragged?.pos];
            destination.Insert((int)DragMemory.hoveringOver?.pos, temp);
            origin.RemoveAt(removePos);
        }
    }

    public struct DragElement
    {
        public int pos;
        public string listname;
        public Vector2 size;
    }
}