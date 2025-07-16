using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using MovieTicketMVC.Services;

namespace MovieTicketMVC.Hubs
{
    public class SeatHub : Hub
    {
        public bool LockSeat(int movieId, string day, string time, string seatId)
        {
            bool ok = SeatLockService.TryLock(movieId, DateTime.Parse(day), time, seatId, Context.ConnectionId);
            
            if (ok)
            {
                Clients.Others.SeatLocked(movieId, day, time, seatId);
            }
            return ok;
        }

        public void UnlockSeat(int movieId, string day, string time, string seatId)
        {
            SeatLockService.Unlock(movieId, DateTime.Parse(day), time, seatId, Context.ConnectionId);
            Clients.Others.SeatUnlocked(movieId, day, time, seatId);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            SeatLockService.ReleaseAll(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }
}