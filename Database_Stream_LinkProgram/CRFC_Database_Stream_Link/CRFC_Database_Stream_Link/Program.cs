using System;
using FireSharp;
using FireSharp.Interfaces;

namespace firebaseConfig
{
    class MainClass
    {
        static void Main(string[] args)
        {
            firebaseConfig ifc = new firebaseConfig();
            IFirebaseClient client;

            client = new FirebaseClient();
        }
    }
}