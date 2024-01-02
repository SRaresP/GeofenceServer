using System.Collections.Generic;

namespace GeofenceServer.Util
{
    class ArrayHelper<T>
    {
        public static bool Add(T[] array, T value, T defaultValue)
        {
            for (int index = 0; index < array.Length; ++index)
            {
                if (EqualityComparer<T>.Default.Equals(array[index], defaultValue))
                {
                    array[index] = value;
                    return true;
                }
            }
            return false;
        }

        public static bool Remove(T[] array, T value, T defaultValue)
        {
            // Find the thing to remove
            for (int index = 0; index < array.Length; ++index)
            {
                if (EqualityComparer<T>.Default.Equals(array[index], value))
                {
                    // Find a replacement for the to-be-removed thing if one exists past it
                    T replacement = defaultValue;
                    for (int replacementIndex = index + 1; replacementIndex < array.Length; ++replacementIndex)
                    {
                        if (EqualityComparer<T>.Default.Equals(array[replacementIndex], defaultValue))
                        {
                            --replacementIndex;
                            replacement = array[replacementIndex];
                            array[replacementIndex] = defaultValue;
                            break;
                        }
                    }

                    if (EqualityComparer<T>.Default.Equals(replacement, value))
                    {
                        // Thing was the only one in the array
                        array[index] = defaultValue;
                    }
                    else
                    {
                        array[index] = replacement;
                    }
                    return true;
                }
                // Don't keep going if the end of the things was reached
                if (EqualityComparer<T>.Default.Equals(array[index], defaultValue))
                {
                    break;
                }
            }
            return false;
        }
    }
}
