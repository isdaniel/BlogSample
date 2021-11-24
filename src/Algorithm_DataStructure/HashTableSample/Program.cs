using System;

namespace HashTableSample
{
    public class Employee{
        public int Id { get; set; }
        public string Name { get; set; }
        public Employee Next { get; set; }
    }

    public class HashTable{
        class EmployeeLinkedList{
            private Employee _header;
            public void Add(Employee e){
                if (_header == null)
                {
                    _header = e;
                    return;
                }

                Employee cur = _header;
                while (cur.Next != null)
                {
                    cur = cur.Next;
                }

                cur.Next = e;
            }

            public Employee Get(int id){

                Employee cur = _header;
                while (cur != null)
                {
                    if (cur.Id == id)
                        return cur;
                    else if (cur.Next == null)
                        break; 

                    cur = cur.Next;
                }

                return null;
            }

            public void Delete(int id){

                if (_header != null && _header.Id == id)
                {
                    _header = _header.Next;
                    return;
                }

                Employee cur = _header;

                while (cur.Next != null)
                {
                    if (cur.Next.Id == id){
                        cur.Next = cur.Next.Next;
                        break;
                    }
                    else if (cur.Next == null)
                        break; 

                    cur = cur.Next;
                }
            }

            public void Modify(Employee e){
                if (_header != null && _header.Id == e.Id)
                {
                    e.Next = _header.Next;
                    _header = e;
                    return;
                }

                Employee cur = _header;

                while (cur.Next != null)
                {
                    if (cur.Next.Id == e.Id){
                        e.Next = cur.Next.Next;
                        cur.Next = e;
                        break;
                    }
                    else if (cur.Next == null)
                        break; 

                    cur = cur.Next;
                }
            }
        }

        EmployeeLinkedList[] _map;
        private readonly int size;

        public HashTable(int size)
        {
            _map = new EmployeeLinkedList[size];
            this.size = size;
            for (int i = 0; i < size; i++)
            {
                _map[i] = new EmployeeLinkedList();
            }
        }

        public void Add(Employee e){
            int idx = HashFunc(e.Id);
            _map[idx].Add(e);
        }

        public Employee GetEmployee(int Id){
            int idx = HashFunc(Id);
            return _map[idx].Get(Id);
        }

        public void Delete(int id){
            int idx = HashFunc(id);
            _map[idx].Delete(id);
        }

        public void Modify(Employee e){
            int idx = HashFunc(e.Id);
            _map[idx].Modify(e);
        }

        private int HashFunc(int id){
            return id % size;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            HashTable table = new HashTable(5);
            table.Add(new Employee(){
                Id = 1,
                Name = "DD"
            });
            table.Add(new Employee(){
                Id = 6,
                Name = "FF"
            });

            var e1 = table.GetEmployee(6);
            var e2 = table.GetEmployee(1);
            var e3 = table.GetEmployee(3);
            table.Delete(1);
            table.Add(new Employee(){
                Id = 11,
                Name = "EE"
            });
            
            table.Add(new Employee(){
                Id = 100,
                Name = "EE"
            });
            table.Modify(new Employee(){Id = 6,Name = "tttttttt"});
            table.Modify(new Employee(){Id = 11,Name = "eaaas"});
            Console.WriteLine("Hello World!");
        }
    }
}
