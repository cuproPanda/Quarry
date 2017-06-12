using System;
using System.Collections.Generic;

namespace Quarry {

  public static class OreDictionary {
    public static OreDictionary<R> From<R>(Dictionary<R, int> oreDictionary) {
      return new OreDictionary<R>(oreDictionary);
    }
  }



  public class OreDictionary<T> {
    private static Random rand = new Random();
    private Dictionary<T, int> oreDictionary;


    public OreDictionary(Dictionary<T, int> oreDictionary) {
      this.oreDictionary = oreDictionary;
    }


    public T TakeOne() {
      // Sorts the weight list
      var sortedWeights = Sort(oreDictionary);

      // Sums all weights
      int sum = 0;
      foreach (var ore in oreDictionary) {
        sum += ore.Value;
      }

      // Randomizes a number from Zero to Sum
      int roll = rand.Next(0, sum);

      // Finds chosen item based on weight
      T selected = sortedWeights[sortedWeights.Count - 1].Key;
      foreach (var ore in sortedWeights) {
        if (roll < ore.Value) {
          selected = ore.Key;
          break;
        }
        roll -= ore.Value;
      }

      // Returns the selected item
      return selected;
    }


    private List<KeyValuePair<T, int>> Sort(Dictionary<T, int> weights) {
      var list = new List<KeyValuePair<T, int>>(weights);

      // Sorts the Weights List for randomization later
      list.Sort(
          delegate (KeyValuePair<T, int> firstPair,
                   KeyValuePair<T, int> nextPair) {
                     return firstPair.Value.CompareTo(nextPair.Value);
                   }
       );

      return list;
    }
  }
}
