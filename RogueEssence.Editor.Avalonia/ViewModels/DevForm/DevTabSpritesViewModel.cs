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
            mv.LoadDataEntries(GraphicsManager.AssetType.Particle, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditBeams_Click()
        {
            BeamEditViewModel mv = new BeamEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.Beam, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditBGs_Click()
        {
            AnimEditViewModel mv = new AnimEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.BG, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }


        public void btnEditEmotes_Click()
        {
            AnimEditViewModel mv = new AnimEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.Icon, editForm);
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
            mv.LoadDataEntries(GraphicsManager.AssetType.Item, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditObjects_Click()
        {
            AnimEditViewModel mv = new AnimEditViewModel();
            Views.AnimEditForm editForm = new Views.AnimEditForm();
            mv.LoadDataEntries(GraphicsManager.AssetType.Object, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }
    }
}
