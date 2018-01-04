using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableList<T>
    {
        private class Updater
        {
            private readonly ObservableList<T> source;
            private readonly IReadOnlyList<T> target;
            private readonly IEqualityComparer<T> comparer;
            private readonly ItemUpdater<T> itemUpdater;
            private readonly int sourceCount, targetCount;
            private readonly int rowCount, columnCount;
            private int[] medMat;
            private int distance;

            public Updater(ObservableList<T> source, IReadOnlyList<T> target, IEqualityComparer<T> comparer, ItemUpdater<T> itemUpdater)
            {
                this.source = source;
                this.target = target;
                this.comparer = comparer;
                this.itemUpdater = itemUpdater;
                this.sourceCount = source.Count;
                this.targetCount = target.Count;
                this.rowCount = this.sourceCount + 1;
                this.columnCount = this.targetCount + 1;
            }

            public int Update()
            {
                if (this.targetCount <= 0)
                {
                    this.source.ClearItems();
                    return this.sourceCount;
                }
                if (this.sourceCount <= 0)
                {
                    this.source.InsertItems(0, this.target);
                    return this.targetCount;
                }
                if (this.sourceCount * this.targetCount > 1_000_000)
                {
                    // Too large
                    Swap();
                    return -1;
                }
                computeMED();
                if (this.sourceCount > this.targetCount ? this.distance == this.sourceCount : this.distance == this.targetCount)
                {
                    // Irrelevant, no need to update step by step
                    Swap();
                    return this.distance;
                }
                if (this.itemUpdater == null)
                    swapMEDFast();
                else
                    swapMEDFull();
                return this.distance;
            }

            private void computeMED()
            {
                var mat = new int[this.rowCount * this.columnCount];
                for (var i = 0; i < this.rowCount; i++)
                {
                    mat[i * this.columnCount] = i;
                }
                for (var j = 0; j < this.columnCount; j++)
                {
                    mat[j] = j;
                }
                // compute
                for (var i = 1; i <= this.sourceCount; i++)
                {
                    for (var j = 1; j <= this.targetCount; j++)
                    {
                        var left = mat[(i - 1) * this.columnCount + j];
                        var up = mat[i * this.columnCount + (j - 1)];
                        var diag = mat[(i - 1) * this.columnCount + (j - 1)];
                        var lu_1 = (left < up ? left : up);
                        lu_1++;
                        if (!this.comparer.Equals(this.source.Items[i - 1], this.target[j - 1]))
                            diag++;
                        mat[i * this.columnCount + j] = (lu_1 < diag) ? lu_1 : diag;
                    }
                }

                this.medMat = mat;
                this.distance = mat[mat.Length - 1];
            }

            private void swapMEDFast()
            {
                // init
                var i = this.sourceCount;
                var j = this.targetCount;
                var remainDistance = this.distance;
                while (remainDistance > 0)
                {
                    // if itemUpdater is null, we can finish iterate when remainDistance becomes 0
                    if (i == 0)
                        // Up only
                        goto INSERTION;
                    else if (j == 0)
                        // Left only
                        goto DELETION;
                    else
                    {
                        var left = this.medMat[(i - 1) * this.columnCount + j];
                        var up = this.medMat[i * this.columnCount + (j - 1)];
                        var diag = this.medMat[(i - 1) * this.columnCount + (j - 1)];
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
                    this.source[i] = this.target[j];
                    remainDistance--;
                    continue;

                    DELETION:
                    i--;
                    this.source.RemoveAt(i);
                    remainDistance--;
                    continue;

                    INSERTION:
                    j--;
                    this.source.Insert(i, this.target[j]);
                    remainDistance--;
                    continue;
                }
            }

            private void swapMEDFull()
            {
                // init
                var i = this.sourceCount;
                var j = this.targetCount;
                var remainDistance = this.distance;
                while (i > 0 || j > 0)
                {
                    // if itemUpdater is not null, we must iterate all common elements regardless of the remainDistance
                    if (i == 0)
                        // Up only
                        goto INSERTION;
                    else if (j == 0)
                        // Left only
                        goto DELETION;
                    else
                    {
                        var left = this.medMat[(i - 1) * this.columnCount + j];
                        var up = this.medMat[i * this.columnCount + (j - 1)];
                        var diag = this.medMat[(i - 1) * this.columnCount + (j - 1)];
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
                    this.itemUpdater(this.source[i], this.target[j]);
                    continue;

                    SUBSTITUTION:
                    i--;
                    j--;
                    this.source[i] = this.target[j];
                    remainDistance--;
                    continue;

                    DELETION:
                    i--;
                    this.source.RemoveAt(i);
                    remainDistance--;
                    continue;

                    INSERTION:
                    j--;
                    this.source.Insert(i, this.target[j]);
                    remainDistance--;
                    continue;
                }
            }

            public void Swap()
            {
                if (this.source.Count > this.target.Count)
                {
                    this.source.SetItems(0, this.target);
                    this.source.RemoveItems(this.target.Count, this.source.Count - this.target.Count);
                }
                else if (this.source.Count < this.target.Count)
                {
                    this.source.SetItems(0, new RangedListView<T>(this.target, 0, this.source.Count));
                    this.source.InsertItems(this.source.Count, new RangedListView<T>(this.target, this.source.Count));
                }
                else
                {
                    this.source.SetItems(0, this.target);
                }
            }
        }
    }
}
