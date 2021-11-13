using System;
using System.Collections;
using System.Linq;

namespace MoviesApp.ViewModels
{
    public class ArtistViewModel:InputArtistViewModel
    {
        public ArtistViewModel() : base()
        {
        }

        public int Id { get; set; }
        /*public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < Movies.Count(); i++)
            {
                yield return Movies.GetEnumerator().Current;
            }
        }*/
    }
}