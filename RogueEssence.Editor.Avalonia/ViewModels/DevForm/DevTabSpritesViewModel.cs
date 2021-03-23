using RogueEssence.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabSpritesViewModel : ViewModelBase
    {

        public void btnEditSprites_Click()
        {
            SpeciesEditViewModel mv = new SpeciesEditViewModel();
            Views.SpeciesEditForm editForm = new Views.SpeciesEditForm();
            mv.LoadFormDataEntries(true, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditPortraits_Click()
        {
            SpeciesEditViewModel mv = new SpeciesEditViewModel();
            Views.SpeciesEditForm editForm = new Views.SpeciesEditForm();
            mv.LoadFormDataEntries(false, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditParticles_Click()
        {
            AnimEditViewModel mv = new AnimEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.VFX, GraphicsManager.PARTICLE_PATTERN, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        //public void btnEditBeams_Click()
        //{

        //}

        public void btnEditBGs_Click()
        {
            AnimEditViewModel mv = new AnimEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.BG, GraphicsManager.BG_PATTERN, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }


        public void btnEditEmotes_Click()
        {
            AnimEditViewModel mv = new AnimEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.Icon, GraphicsManager.ICON_PATTERN, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditTiles_Click()
        {
            TilesetEditViewModel mv = new TilesetEditViewModel();
            Views.TilesetEditForm editForm = new Views.TilesetEditForm();
            mv.LoadDataEntries(editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditItems_Click()
        {
            AnimEditViewModel mv = new AnimEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.Item, GraphicsManager.ITEM_PATTERN, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditObjects_Click()
        {
            AnimEditViewModel mv = new AnimEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.Object, GraphicsManager.OBJECT_PATTERN, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }
    }
}
