// 
// Sport.cs
//  
// Author:
//       Trent McPheron <twilightinzero@gmail.com>
// 
// Copyright (c) 2009-2011 Trent McPheron
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Arbiter {

// A simple data structure representing a sport.
public struct Sport {
    
    ///////////////////
    // Properties
    ///////////////////
    
    public string   ShortName    { get; private set; }
    public bool     Fancies      { get; private set; }
    public bool     Feints       { get; private set; }
    public bool     Focuses      { get; private set; }
    public bool     Advantages   { get; private set; }
    public string   ScoreFormat  { get; private set; }
    public char[,]  Matrix       { get; private set; }
    public string[] Abbrev       { get; private set; }
    public string[] Moves        { get; private set; }
    
    
    ///////////////////
    // Constructor
    ///////////////////
    
    public Sport (string sportName) : this () {
        // Load the embedded config for the sport.
        StreamReader sr = new StreamReader(
         Assembly.GetExecutingAssembly().GetManifestResourceStream("sport-" +
         sportName + ".cfg"));
        
        // Load the comma separated header.
        string[] header = sr.ReadLine().Split(',');
        ShortName = header[0];
        Fancies = Boolean.Parse(header[1]);
        Feints = Boolean.Parse(header[2]);
        Focuses = Boolean.Parse(header[3]);
        Advantages = Boolean.Parse(header[4]);
        ScoreFormat = header[5];
        int m = Int32.Parse(header[6]); // Number of moves.
        
        // Read the matrix.
        Matrix = new char[m,m];
        for (int a = 0; a < m; a++) {
            for (int b = 0; b < m; b++) {
                Matrix[a,b] = (char)sr.Read();
            }
            sr.Read();  // Reads the CR
            sr.Read();  // then the LF.
        }
        
        // Verify the matrix.
        bool error = false;
        int errA = 0, errB = 0;
        for (int a = 0; a < m; a++)
        for (int b = 0; b < m; b++) {
            bool e = false;
            switch (Matrix[a,b]) {
            case 'A':
                e = Matrix[b,a] != 'B';
                break;
            case 'a':
                e = Matrix[b,a] != 'b';
                break;
            case 'B':
                e = Matrix[b,a] != 'A';
                break;
            case 'b':
                e = Matrix[b,a] != 'a';
                break;
            case '1':
                e = Matrix[b,a] != '1';
                break;
            case '+':
                e = Matrix[b,a] != '+';
                break;
            case '0':
                e = Matrix[b,a] != '0';
                break;
            }
            if (e) {
                errA = a; errB = b;
            }
            error = error || e;
        }
        
        if (error) {
            Console.WriteLine("A consistency error was detected in the " +
             sportName + " matrix\n at "+ errA.ToString() + "," +
             errB.ToString() + ". " + "Please inform the author of this " +
             "error immediately.");
        }
        
        // Read the abbreviations.
        Abbrev = sr.ReadLine().Split(',');
        
        // Read the move list.
        Moves = new string[m];
        for (int l = 0; l < m; l++) {
            Moves[l] = sr.ReadLine();
        }
    }
    
    
    ///////////////////
    // Methods
    ///////////////////
    
    // Evaluates a pair of moves and modifiers.
    public void Resolve (int moveA, bool fancyA, bool feintA, bool focusA,
     int moveB, bool fancyB, bool feintB, bool focusB, out float resultA,
     out float resultB) {
        // Initialize the output vars.
        resultA = 0;
        resultB = 0;
        
        // The individual moves are used as
        // coordinates on the matrix
        switch (Matrix[moveA, moveB]) {
        case 'A':  // A scores.
            if (!feintA) {
                resultA = focusA ? 1.5f : 1;
            }
            break;
        case 'B':  // B scores.
            if (!feintB) {
                resultB = focusB ? 1.5f : 1;
            }
            break;
        case 'a':  // A gets advantage.
            if (feintB) {
                resultB = 1;
            } else {
                resultA = (fancyA || focusA) ? 1 : 0.5f;
            }
            break;
        case 'b':  // B gets advantage.
            if (feintA) {
                resultA = 1;
            } else {
                resultB = (fancyB || focusB) ? 1 : 0.5f;
            }
            break;
        case '1':  // Both score.
            if (!feintA) {
                resultA = focusA ? 1.5f : 1;
            }
            if (!feintB) {
                resultB = focusB ? 1.5f : 1;
            }
            break;
        case '+':  // Magic only: dual advantage.
            resultA = focusA ? 1 : 0.5f;
            resultB = focusB ? 1 : 0.5f;
            break;
        case '0':  // Null round.
            break;
        }
    }
    
}

}
