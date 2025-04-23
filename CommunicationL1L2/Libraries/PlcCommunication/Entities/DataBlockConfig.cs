using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommunication.Model
{
    /// <summary>
    /// Describes how a single circular buffer DB is laid out.
    /// </summary>
    public class DataBlockConfig
    {
        // ─── IDENTITY ─────────────────────────────────────────────────
        public required string Name { get; init; }
        public required ushort Id { get; init; }

        // ─── HEADER ───────────────────────────────────────────────────
        public required int ChangeCounterStart { get; init; }   // e.g. byte 0
        public required int BufferPointerStart { get; init; }   // e.g. byte 2
        public required int MaxElementsStart { get; init; }     // e.g. byte 4

        // ─── BODY ─────────────────────────────────────────────────────
        public required int ContentStart { get; init; }         // e.g. byte 6
        public required short Offset { get; init; }             // bytes per element

        /// <summary>
        /// Number of buffer‐elements (i.e. the circular buffer depth).
        /// </summary>
        public required short Size { get; init; }

        // ─── FOOTER ───────────────────────────────────────────────────
        public required int AuxCounterStart { get; init; }   

        // ─── MAPPING ──────────────────────────────────────────────────
        /// <summary>CLR type to instantiate for non‑generic reads.</summary>
        public required Type ModelType { get; init; }
    }
}
