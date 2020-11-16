using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;
using ReactiveUI;
using RogueElements;

namespace RogueEssence.Dev.ViewModels
{
    public class MapResizeViewModel : ViewModelBase
    {
        public MapResizeViewModel(int mapWidth, int mapHeight)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.resizeDir = Dir8.None;
        }

        private int mapWidth;
        public int MapWidth
        {
            get { return mapWidth; }
            set
            {
                this.RaiseAndSetIfChanged(ref mapWidth, value);
            }
        }

        private int mapHeight;
        public int MapHeight
        {
            get { return mapHeight; }
            set
            {
                this.RaiseAndSetIfChanged(ref mapHeight, value);
            }
        }

        private Dir8 resizeDir;
        public Dir8 ResizeDir
        {
            get { return resizeDir; }
            set
            {
                this.RaiseAndSetIfChanged(ref resizeDir, value);
            }
        }

        public void btnUpLeft_Click()
        {
            ResizeDir = Dir8.UpLeft;
        }
        public void btnUp_Click()
        {
            ResizeDir = Dir8.Up;
        }
        public void btnUpRight_Click()
        {
            ResizeDir = Dir8.UpRight;
        }

        public void btnLeft_Click()
        {
            ResizeDir = Dir8.Left;
        }
        public void btnNone_Click()
        {
            ResizeDir = Dir8.None;
        }
        public void btnRight_Click()
        {
            ResizeDir = Dir8.Right;
        }

        public void btnDownLeft_Click()
        {
            ResizeDir = Dir8.DownLeft;
        }
        public void btnDown_Click()
        {
            ResizeDir = Dir8.Down;
        }
        public void btnDownRight_Click()
        {
            ResizeDir = Dir8.DownRight;
        }
    }
}
