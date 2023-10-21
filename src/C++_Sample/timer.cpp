#include <iostream>
#include <chrono>
#include <sys/epoll.h>
#include <functional> //c++ 11 support
#include <set>
#include <memory>

using namespace std;
using namespace std::chrono;

struct  NodeBase
{
    time_t expire; //過期時間
    int64_t id;
};


struct TimerNode : NodeBase {
    using Callback = std::function<void (const TimerNode &node)>;
    Callback func;
    TimerNode(time_t exp, int64_t id,Callback func)  {
        this->expire = exp;
        this->func = func;
        this->id = id;
    }
};

bool operator < (const NodeBase &lhd,const NodeBase &rhd) {
    // if(lhd.expire < rhd.expire){
    //     return true;
    // } else if (lhd.expire > rhd.expire){
    //     return false;
    // }

    return  lhd.expire < rhd.expire || lhd.id < rhd.id;
}

class CTimer{
public:
    static time_t GetTick(){
        auto sc = time_point_cast<chrono::milliseconds>(chrono::steady_clock::now());
        auto tmp = duration_cast<chrono::milliseconds>(sc.time_since_epoch());
        return tmp.count();
    }

    NodeBase AddTimer(time_t msec,TimerNode::Callback func){
        time_t expire = GetTick() + msec;
        auto ele = timer.emplace(expire, GenerateId(),func);
        return *ele.first;
        //return static_cast<NodeBase>*ele.first;
    }

    bool DelTimer(NodeBase &node){
        auto iter = timer.find(node); //在 C++14 後 才可以透過等價 key 尋找
        if (iter != timer.end())
        {
            timer.erase(iter);
            return true;
        }
        return false;
        
    }

    bool CheckTimer(){
        auto iter = timer.begin();
        if(iter != timer.end() && iter->expire <= GetTick()){
            iter->func(*iter);
            timer.erase(iter);
            return true;
        }

        return false;
    }

    time_t TimeToSleep(){
        auto iter = timer.begin();
        if(iter == timer.end()){
            return -1;
        }
        time_t nextTrigger = iter->expire - GetTick();
        return nextTrigger  > 0 ? nextTrigger  : 0;
    }

private:
    set<TimerNode,std::less<>> timer;
    static int64_t GenerateId(){
        return gid++;
    }
    static int64_t gid;
};

int64_t CTimer::gid = 0;

int main(){
    int epfd = epoll_create(1);

    //when system call copy data from kernal to user mode
    epoll_event ev[64] = {0};

    unique_ptr<CTimer> timer = make_unique<CTimer>();
    int i = 0;
    timer->AddTimer(3000,[&](const TimerNode &node){
        cout << CTimer::GetTick() << " node Id "<< node.id << " i " << i++ << endl;
    });
    
    timer->AddTimer(1000,[&](const TimerNode &node){
        cout << CTimer::GetTick() << " node Id "<< node.id << " i " << i++ << endl;
    });

    auto node = timer->AddTimer(2100,[&](const TimerNode &node){
        cout << CTimer::GetTick() << "Del node Id "<< node.id << " i " << i++ << endl;
    });

    timer->DelTimer(node);

    while (true)
    {
        //多路復用 system call, reduce system call times
        //select()
        //epoll
        int n = epoll_wait(epfd,ev,64, timer->TimeToSleep());
        for (int i = 0; i < n; i++)
        {
            //處理 networking event
        }
        
        //handle timer
        while(timer->CheckTimer());
    }
    

    return 0;
}