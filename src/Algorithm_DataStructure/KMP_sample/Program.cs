
System.Console.WriteLine(KMP("abababcaa","ababcc"));

bool KMP(string source,string subStr){
    int i = 0;
    int j = 0;
    int alen = source.Length;
    int blen = subStr.Length;
    var next = Build_NextTable(subStr);
    while (i < alen && j < blen)
    {
        if(source[i] == subStr[j]){
            i++;
            j++;
        } else if (j > 0){
            j = next[j - 1];
        } else {
            i++;
        }

        if(j == blen){
            return true;
        }
    }

    return false;
}

//PMT
int[] Build_NextTable(string s){
    int[] res = new int[s.Length];
    int prefix_len = 0;
    for(int i = 1; i < s.Length; i++)
    {
        if(s[prefix_len] == s[i]){
            prefix_len++;
            res[i] = prefix_len;
        } else {
            prefix_len = prefix_len - 1 < 0 ? 0 : res[prefix_len - 1];
            if(prefix_len == 0){
                res[i] = 0;
            }
        }
    }
    return res;
}
