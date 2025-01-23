using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Ksiegarnia.Models.ViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public int AuthorId { get; set; }
        public IEnumerable<SelectListItem> Authors { get; set; }
        [Required]
        public int PublisherId { get; set; }
        public IEnumerable<SelectListItem> Publishers { get; set; }
        public double Price { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public List<int> SelectedTagIds { get; set; }
        public IEnumerable<SelectListItem> Tags { get; set; }
    }

}
