using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class TileBrowserViewModel : ViewModelBase
    {
        public TileBrowserViewModel()
        {

        }

        public string CurrentTileset;

        public void SelectTileset(string sheetName)
        {
            //slbTilesets.SearchText = "";
            //slbTilesets.SelectedIndex = tileIndices.FindIndex(str => (str == sheetName));
        }

        public void SetTileSize(int tileSize)
        {
            //this.tileSize = tileSize;
            //UpdateTilesList();
        }

        public void UpdateTilesList()
        {
            //tileIndices.Clear();
            //slbTilesets.Clear();

            //foreach (string name in GraphicsManager.TileIndex.Nodes.Keys)
            //{
            //    if (GraphicsManager.TileIndex.GetTileSize(name) == tileSize)
            //    {
            //        tileIndices.Add(name);
            //        slbTilesets.AddItem(name);
            //    }
            //}

            //if (tileIndices.Count > 0)
            //{

            //    chosenTileset = tileIndices[0];
            //    currentTileset = tileIndices[0];

            //    slbTilesets.SelectedIndex = 0;
            //}
            //else
            //{
            //    chosenTileset = "";
            //    currentTileset = "";
            //}
            //chosenTile = Loc.Zero;
            //multiSelect = Loc.One;

            //RefreshAnimControls();

            //tilePreview.SetChosenAnim(new TileBrush(new TileLayer(chosenTile, chosenTileset), multiSelect));

            ////refresh
            //RefreshTileSelect();
        }
    }
}
