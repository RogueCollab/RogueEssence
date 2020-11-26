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

    }
}
