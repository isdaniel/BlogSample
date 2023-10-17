List<int> res = new List<int>();
int[] arr1 = new int[]{1,};
int[] arr2 = new int[]{9};

AddTwoNumbers(arr1,arr1.Length - 1,arr2,arr2.Length - 1);
System.Console.WriteLine(string.Join('\0',res));


void AddTwoNumbers(int[] arr1,int idx1, int[] arr2,int idx2,int carry = 0) {
    if(idx1 < 0 && idx2 < 0 && carry == 0){
        return;
    }

    int total = (idx1 >= 0 ? arr1[idx1] : 0) + (idx2 >= 0 ? arr2[idx2] : 0)+ carry; 

    carry = total / 10;
    res.Insert(0,total % 10);
    AddTwoNumbers(arr1,idx1-1,arr2,idx2-1,carry);
}