using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableList<T>
    {
        static class Updater
        {
            public static int Update(ObservableList<T> source, IReadOnlyList<T> target, IEqualityComparer<T> comparer)
            {
                var sourceCount = source.Count;
                var targetCount = target.Count;
                if (targetCount <= 0)
                {
                    source.ClearItems();
                    return sourceCount;
                }
                if (sourceCount == 0)
                {
                    source.InsertItems(0, target);
                    return targetCount;
                }
                if (sourceCount * targetCount > 1_000_000)
                {
                    // Too large
                    Swap(source, target);
                    return -1;
                }
                var mat = computeMED(source, target, comparer);
                var distance = mat[sourceCount, targetCount];
                if (sourceCount > targetCount ? distance == sourceCount : distance == targetCount)
                {
                    // Irrelevant, no need to update step by step
                    Swap(source, target);
                    return distance;
                }
                swapMED(source, target, mat);
                return distance;
            }

            private static int[,] computeMED(ObservableList<T> source, IReadOnlyList<T> target, IEqualityComparer<T> comparer)
            {
                var sourceCount = source.Count;
                var targetCount = target.Count;
                // init
                var mat = new int[sourceCount + 1, targetCount + 1];
                for (var i = 0; i <= sourceCount; i++)
                {
                    mat[i, 0] = i;
                }
                for (var j = 0; j <= targetCount; j++)
                {
                    mat[0, j] = j;
                }
                // compute
                for (var i = 1; i <= sourceCount; i++)
                {
                    for (var j = 1; j <= targetCount; j++)
                    {
                        var left = mat[i - 1, j];
                        var up = mat[i, j - 1];
                        var diag = mat[i - 1, j - 1];
                        var lu_1 = (left < up ? left : up);
                        lu_1++;
                        if (!comparer.Equals(source.Items[i - 1], target[j - 1]))
                            diag++;
                        mat[i, j] = (lu_1 < diag) ? lu_1 : diag;
                    }
                }

                return mat;
            }

            private static void swapMED(ObservableList<T> source, IReadOnlyList<T> target, int[,] medMat)
            {
                var i = source.Count;
                var j = target.Count;
                var remainDistance = medMat[i, j];
                while (remainDistance > 0)
                {
                    if (i == 0)
                        // Up only
                        goto INSERTION;
                    else if (j == 0)
                        // Left only
                        goto DELETION;
                    else
                    {
                        var left = medMat[i - 1, j];
                        var up = medMat[i, j - 1];
                        var diag = medMat[i - 1, j - 1];
                        if (diag <= left && diag <= up)
                        {
                            if (remainDistance == diag)
                                goto DIAG_NO_OPERATION;
                            else
                                goto SUBSTITUTION;
                        }
                        else if (left == remainDistance - 1)
                        {
                            goto DELETION;
                        }
                        else
                        {
                            goto INSERTION;
                        }
                    }

                DIAG_NO_OPERATION:
                    i--;
                    j--;
                    continue;

                SUBSTITUTION:
                    i--;
                    j--;
                    source[i] = target[j];
                    remainDistance--;
                    continue;

                DELETION:
                    i--;
                    source.RemoveAt(i);
                    remainDistance--;
                    continue;

                INSERTION:
                    j--;
                    source.Insert(i, target[j]);
                    remainDistance--;
                    continue;
                }
            }

            public static void Swap(ObservableList<T> source, IReadOnlyList<T> target)
            {
                if (source.Count > target.Count)
                {
                    source.SetItems(0, target);
                    source.RemoveItems(target.Count, source.Count - target.Count);
                }
                else if (source.Count < target.Count)
                {
                    source.SetItems(0, new RangedListView<T>(target, 0, source.Count));
                    source.InsertItems(source.Count, new RangedListView<T>(target, source.Count));
                }
                else
                {
                    source.SetItems(0, target);
                }
            }
        }
    }
}
