using System.ComponentModel.DataAnnotations;
namespace MEH.Models
{
    public enum MitarbeiterRolle
    {
        [Display(Name = "Mitarbeiter")]
        Mitarbeiter = 0,

        [Display(Name = "Support")]
        Support = 1,
    }
}