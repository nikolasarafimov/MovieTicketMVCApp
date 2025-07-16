using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieTicketMVC.Services
{
    public static class SeatLockService
    {
        private static readonly ConcurrentDictionary<string, string> _locks
            = new ConcurrentDictionary<string, string>();

        private static string BuildKey(int movieId, DateTime day, string time, string seatId)
            => $"{movieId}_{day:yyyyMMdd}_{time}_{seatId}";

        public static bool TryLock(int movieId, DateTime day, string time, string seatId, string connectionId)
        {
            var key = BuildKey(movieId, day, time, seatId);
            return _locks.TryAdd(key, connectionId);
        }

        public static void Unlock(int movieId, DateTime day, string time, string seatId, string connectionId)
        {
            var key = BuildKey(movieId, day, time, seatId);

            if (_locks.TryGetValue(key, out var owner) && owner == connectionId)
            {
                _locks.TryRemove(key, out _);
            }
        }

        public static void ReleaseAll(string connectionId)
        {
            var theirs = _locks
                .Where(kvp => kvp.Value == connectionId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in theirs)
            {
                _locks.TryRemove(key, out _);
            }
        }
    }
}