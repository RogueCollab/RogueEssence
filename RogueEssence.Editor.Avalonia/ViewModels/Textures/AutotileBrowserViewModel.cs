using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using RogueElements;
using RogueEssence.Dungeon;
using ReactiveUI;
using RogueEssence.Content;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using RogueEssence.Data;

namespace RogueEssence.Dev.ViewModels
{
    public class AutotileBrowserViewModel : ViewModelBase
    {
        public AutotileBrowserViewModel()
        {
            preview = TileFrame.Empty;
            borderPreview = TileFrame.Empty;


            Autotiles = new SearchListBoxViewModel();
            Autotiles.DataName = "Autotiles:";
            Autotiles.SelectedIndexChanged += Autotiles_SelectedIndexChanged;

            BorderAutotiles = new SearchListBoxViewModel();
            BorderAutotiles.DataName = "Border Autotiles:";
            BorderAutotiles.SelectedIndexChanged += BorderAutotiles_SelectedIndexChanged;

            UpdateAutotilesList();
        }

        public SearchListBoxViewModel Autotiles { get; set; }

        public SearchListBoxViewModel BorderAutotiles { get; set; }


        /// <summary>
        /// The current tile being previewed
        /// </summary>
        private TileFrame preview;
        public TileFrame Preview
        {
            get => preview;
            set => this.SetIfChanged(ref preview, value);
        }


        /// <summary>
        /// The current tile being previewed
        /// </summary>
        private TileFrame borderPreview;
        public TileFrame BorderPreview
        {
            get => borderPreview;
            set => this.SetIfChanged(ref borderPreview, value);
        }

        public int TileSize
        {
            get => ZoneManager.Instance.CurrentGround.TileSize;
            set
            {
                this.RaisePropertyChanged();
                UpdateAutotilesList();
            }
        }


        /// <summary>
        /// The full draw data to be used as a brush. Computed.
        /// </summary>
        public TileBrush GetBrush()
        {
            return new TileBrush(Autotiles.InternalIndex-1, BorderAutotiles.InternalIndex - 1);
        }

        public void SetBrush(AutoTile autotile)
        {
            Autotiles.SearchText = "";
            Autotiles.SelectedSearchIndex = autotile.AutoTileset + 1;
            BorderAutotiles.SearchText = "";
            BorderAutotiles.SelectedSearchIndex = autotile.BorderTileset + 1;
        }


        public void UpdateAutotilesList()
        {
            if (Design.IsDesignMode)
                return;

            Autotiles.Clear();
            BorderAutotiles.Clear();

            Autotiles.AddItem("---");
            BorderAutotiles.AddItem("---");

            for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.AutoTile].Count; ii++)
            {
                EntrySummary entry = DataManager.Instance.DataIndices[DataManager.DataType.AutoTile].Entries[ii];
                //TODO: autotiles need tile sizes too, to compare
                if (24 == TileSize)
                {
                    Autotiles.AddItem(ii.ToString("D3") + ": " + entry.Name.ToLocal());
                    BorderAutotiles.AddItem(ii.ToString("D3") + ": " + entry.Name.ToLocal());
                }
            }

        }


        private void Autotiles_SelectedIndexChanged()
        {
            if (Autotiles.InternalIndex == 0)
            {
                Preview = TileFrame.Empty;
                return;
            }
            AutoTileData autoTile = DataManager.Instance.GetAutoTile(Autotiles.InternalIndex - 1);
            Preview = autoTile.Tiles.Generic[0].Frames[0];
        }

        private void BorderAutotiles_SelectedIndexChanged()
        {
            if (BorderAutotiles.InternalIndex == 0)
            {
                BorderPreview = TileFrame.Empty;
                return;
            }
            AutoTileData autoTile = DataManager.Instance.GetAutoTile(BorderAutotiles.InternalIndex - 1);
            BorderPreview = autoTile.Tiles.Generic[0].Frames[0];
        }


    }
}
