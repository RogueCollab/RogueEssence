using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;
using RogueElements;
using System.Collections;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Dev.ViewModels
{
    public class TileBoxViewModel : ViewModelBase
    {
        public AutoTile Tile { get; private set; }

        private TileFrame preview;
        public TileFrame Preview
        {
            get => preview;
            set => this.SetIfChanged(ref preview, value);
        }

        public delegate void EditElementOp(AutoTile element);
        public delegate void ElementOp(AutoTile element, EditElementOp op);

        public event ElementOp OnEditItem;
        public event Action OnMemberChanged;

        public TileBoxViewModel()
        {
            preview = TileFrame.Empty;
        }

        public void LoadFromSource(AutoTile source)
        {
            Tile = source;
            updatePreview();
        }

        private void updateSource(AutoTile source)
        {
            LoadFromSource(source);
            updatePreview();
            OnMemberChanged?.Invoke();
        }

        private void updatePreview()
        {
            //TODO: draw all layers, and draw the animations too
            if (!String.IsNullOrEmpty(Tile.AutoTileset))
            {
                AutoTileData autoTile = DataManager.Instance.GetAutoTile(Tile.AutoTileset);
                List<TileLayer> layer = autoTile.Tiles.GetLayers(-1);
                Preview = layer[0].Frames[0];
            }
            else if (Tile.Layers.Count > 0)
                Preview = Tile.Layers[0].Frames[0];
            else
                Preview = TileFrame.Empty;
        }

        public void btnEdit_Click()
        {
            AutoTile element = Tile;
            OnEditItem?.Invoke(element, updateSource);
        }
    }
}
