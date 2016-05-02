// Copyright 2014 Frank A. Krueger
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var NObject = (function () {
    function NObject() {
    }
    NObject.prototype.Equals = function (other) {
        return this === other;
    };
    NObject.prototype.GetHashCode = function () {
        return NString.GetHashCode(this.toString());
    };
    NObject.prototype.ToString = function () {
        return this.GetType().Name;
    };
    NObject.prototype.toString = function () {
        return this.ToString();
    };
    NObject.prototype.GetType = function () {
        return new Type(this.constructor.toString().match(/function (\w*)/)[1]);
    };
    NObject.ReferenceEquals = function (x, y) {
        return x === y;
    };
    NObject.GenericEquals = function (x, y) {
        if (typeof x === "object")
            return x.Equals(y);
        return x === y;
    };
    NObject.GenericToString = function (x) {
        if (typeof x === "object")
            return x.ToString();
        return x.toString();
    };
    NObject.GenericGetHashCode = function (x) {
        if (typeof x === "object")
            return x.GetHashCode();
        return NString.GetHashCode(this.toString());
    };
    return NObject;
}());
var Exception = (function (_super) {
    __extends(Exception, _super);
    function Exception(message) {
        if (message === void 0) { message = ""; }
        _super.call(this);
        this.Message = message;
    }
    Exception.prototype.ToString = function () {
        return "Exception: " + this.Message;
    };
    return Exception;
}(NObject));
var NEvent = (function () {
    function NEvent() {
        this.listeners = new Array();
    }
    NEvent.prototype.Add = function (listener) {
        this.listeners.push(listener);
    };
    NEvent.prototype.Remove = function (listener) {
        var index = this.listeners.indexOf(listener);
        if (index < 0)
            return;
        this.listeners.splice(index, 1);
    };
    NEvent.prototype.ToMulticastFunction = function () {
        if (this.listeners.length === 0)
            return null;
        return function () {
            for (var i in this.listeners) {
                this.listeners[i].call(arguments);
            }
        };
    };
    return NEvent;
}());
var NArray = (function () {
    function NArray() {
    }
    NArray.IndexOf = function (values, value) {
        var i, n = values.length;
        for (i = 0; i < n; i++) {
            if (values[i] === value)
                return i;
        }
        return -1;
    };
    NArray.ToEnumerable = function (array) {
        return new Array_Enumerable(array);
    };
    NArray.Resize = function (parray, newLength) {
        if (parray[0] === null) {
            parray[0] = new Array(newLength);
            return;
        }
        if (parray[0].length === newLength) {
            return;
        }
        throw new NotImplementedException();
    };
    NArray.Copy = function (sourceArray, sourceIndex, destinationArray, destinationIndex, length) {
        for (var index = 0; index < length; index++) {
            destinationArray[index + destinationIndex] = sourceArray[index + sourceIndex];
        }
    };
    NArray.Uint8ArrayCopy = function (sourceArray, sourceIndex, destinationArray, destinationIndex, length) {
        for (var index = 0; index < length; index++) {
            destinationArray[index + destinationIndex] = sourceArray[index + sourceIndex];
        }
    };
    NArray.Clear = function (array, index, length) {
        for (var i = 0; i < length; i++) {
            array[i] = null;
        }
    };
    NArray.Uint8ArrayClear = function (array, index, length) {
        for (var i = 0; i < length; i++) {
            array[i] = 0;
        }
    };
    return NArray;
}());
var NNumber = (function () {
    function NNumber() {
    }
    NNumber.Parse = function (text, styleOrProvider, provider) {
        return parseFloat(text);
    };
    NNumber.ToString = function (num, providerOrFormat, provider) {
        return num.toString();
    };
    NNumber.GetHashCode = function (num) {
        return num;
    };
    NNumber.IsInfinity = function (num) {
        return num === Infinity;
    };
    NNumber.TryParse = function (str, pvalue) {
        try {
            pvalue[0] = parseFloat(str);
            return true;
        }
        catch (ex) {
            pvalue[0] = 0;
            return false;
        }
    };
    NNumber.IsNaN = function (num) {
        return isNaN(num);
    };
    return NNumber;
}());
var NBoolean = (function () {
    function NBoolean() {
    }
    NBoolean.TryParse = function (str, pbool) {
        throw new NotImplementedException;
    };
    NBoolean.GetHashCode = function (bool) {
        return bool ? 1 : 0;
    };
    return NBoolean;
}());
var NChar = (function () {
    function NChar() {
    }
    NChar.IsWhiteSpace = function (ch) {
        return ch === 32 || (ch >= 9 && ch <= 13) || ch === 133 || ch === 160;
    };
    NChar.IsLetter = function (ch) {
        return (65 <= ch && ch <= 90) || (97 <= ch && ch <= 122) || (ch >= 128 && ch !== 133 && ch !== 160);
    };
    NChar.IsLetterOrDigit = function (ch) {
        return (48 <= ch && ch <= 57) || (65 <= ch && ch <= 90) || (97 <= ch && ch <= 122) || (ch >= 128 && ch !== 133 && ch !== 160);
    };
    NChar.IsDigit = function (chOrStr, index) {
        if (arguments.length == 1) {
            return 48 <= chOrStr && chOrStr <= 57;
        }
        else {
            var ch = chOrStr.charCodeAt(index);
            return 48 <= ch && ch <= 57;
        }
    };
    return NChar;
}());
var NString = (function () {
    function NString() {
    }
    NString.IndexOf = function (str, chOrSub, startIndex) {
        var sub;
        if (chOrSub.constructor == Number) {
            sub = String.fromCharCode(chOrSub);
        }
        else {
            sub = chOrSub;
        }
        return str.indexOf(sub);
    };
    NString.IndexOfAny = function (str, subs) {
        for (var i = 0; i < str.length; ++i) {
            var c = str.charCodeAt(i);
            for (var j = 0; j < subs.length; ++j) {
                if (c == subs[j])
                    return i;
            }
        }
        return -1;
    };
    NString.GetHashCode = function (str) {
        var hash = 0, i, l, ch;
        if (str.length == 0)
            return hash;
        for (i = 0, l = str.length; i < l; i++) {
            ch = str.charCodeAt(i);
            hash = ((hash << 5) - hash) + ch;
            hash |= 0; // Convert to 32bit integer
        }
        return hash;
    };
    NString.Replace = function (str, pattern, replacement) {
        var ps = (pattern.constructor === Number) ? String.fromCharCode(pattern) : pattern;
        var rs = (replacement.constructor === Number) ? String.fromCharCode(replacement) : replacement;
        return str.replace(ps, rs);
    };
    NString.Substring = function (str, startIndex, length) {
        if (length === void 0) { length = -1; }
        return length < 0 ? str.substr(startIndex) : str.substr(startIndex, length);
    };
    /*static Remove(str: string, startIndex: number): string*/
    NString.Remove = function (str, startIndex, length) {
        if (typeof str === undefined) {
            return str.substring(startIndex);
        }
        else {
            return str.substring(0, startIndex - 1) + str.substring(startIndex + length);
        }
    };
    /*static Remove(str: string, startIndex: number, length?: number): string
    {
        throw new NotImplementedException(); // do we care that ts->js compiler will get rid of this syntactic sugar?
    }*/
    NString.Trim = function (str) {
        return str.trim();
    };
    NString.TrimStart = function (str, trimChars) {
        throw new NotImplementedException();
    };
    NString.TrimEnd = function (str, trimChars) {
        throw new NotImplementedException();
    };
    NString.ToUpperInvariant = function (str) {
        return str.toUpperCase();
    };
    NString.ToLowerInvariant = function (str) {
        return str.toLowerCase();
    };
    NString.Contains = function (str, sub) {
        return str.indexOf(sub) >= 0;
    };
    NString.StartsWith = function (str, sub, comp) {
        return str.indexOf(sub) === 0;
    };
    NString.EndsWith = function (str, sub, comp) {
        return str.indexOf(sub) === str.length - sub.length;
    };
    NString.Format = function (format, arg0, arg1, arg2, arg3, arg4, arg5) {
        if (arg0.constructor === Array) {
            var s = format, i = arg0.length;
            while (i--) {
                s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arg0[i]);
            }
            return s;
        }
        else {
            var args = [arg0, arg1, arg2, arg3, arg4, arg5];
            return this.Format(format, args, null, null, null, null, null);
        }
    };
    NString.IsNullOrEmpty = function (str) {
        return !str;
    };
    NString.Join = function (separator, parts) {
        throw new NotImplementedException();
    };
    NString.Concat = function (parts) {
        throw new NotImplementedException();
    };
    NString.FromChars = function (chOrChars, count) {
        if (count === void 0) { count = 1; }
        if (chOrChars.constructor === Number) {
            var r = String.fromCharCode(chOrChars);
            for (var i = 2; i < count; i++) {
                r += String.fromCharCode(chOrChars);
            }
            return r;
        }
        throw new NotImplementedException();
    };
    NString.Empty = "";
    return NString;
}());
var StringComparison;
(function (StringComparison) {
    StringComparison[StringComparison["InvariantCultureIgnoreCase"] = 0] = "InvariantCultureIgnoreCase";
    StringComparison[StringComparison["Ordinal"] = 1] = "Ordinal";
})(StringComparison || (StringComparison = {}));
var NMath = (function (_super) {
    __extends(NMath, _super);
    function NMath() {
        _super.apply(this, arguments);
    }
    NMath.Truncate = function (value) {
        return value >= 0 ? Math.floor(value) : Math.ceil(value);
    };
    NMath.Log = function (a, newBase) {
        if (newBase === void 0) { newBase = Math.E; }
        if (newBase === Math.E)
            return Math.log(a);
        return Math.log(a) / Math.log(newBase);
    };
    NMath.Round = function (a, decimals) {
        if (decimals === void 0) { decimals = 0; }
        if (decimals === 0)
            return Math.round(a);
        var s = Math.pow(10, decimals);
        return Math.round(a * s) / s;
    };
    NMath.Cosh = function (x) {
        throw new NotImplementedException();
    };
    NMath.Sinh = function (x) {
        throw new NotImplementedException();
    };
    NMath.Tanh = function (x) {
        throw new NotImplementedException();
    };
    return NMath;
}(NObject));
//
// System
//
var Type = (function (_super) {
    __extends(Type, _super);
    function Type(Name) {
        _super.call(this);
        this.Name = Name;
    }
    Type.prototype.Equals = function (obj) {
        return (obj instanceof Type) && (obj.Name === this.Name);
    };
    return Type;
}(NObject));
var Nullable = (function (_super) {
    __extends(Nullable, _super);
    function Nullable(value) {
        if (value === void 0) { value = null; }
        _super.call(this);
        this.Value = value;
    }
    Object.defineProperty(Nullable.prototype, "HasValue", {
        get: function () { return this.Value != null; },
        enumerable: true,
        configurable: true
    });
    return Nullable;
}(NObject));
var DateTimeKind;
(function (DateTimeKind) {
    DateTimeKind[DateTimeKind["Local"] = 0] = "Local";
    DateTimeKind[DateTimeKind["Unspecified"] = 1] = "Unspecified";
    DateTimeKind[DateTimeKind["Utc"] = 2] = "Utc";
})(DateTimeKind || (DateTimeKind = {}));
var DayOfWeek;
(function (DayOfWeek) {
    DayOfWeek[DayOfWeek["Sunday"] = 0] = "Sunday";
    DayOfWeek[DayOfWeek["Monday"] = 1] = "Monday";
    DayOfWeek[DayOfWeek["Tuesday"] = 2] = "Tuesday";
    DayOfWeek[DayOfWeek["Wednesday"] = 3] = "Wednesday";
    DayOfWeek[DayOfWeek["Thursday"] = 4] = "Thursday";
    DayOfWeek[DayOfWeek["Friday"] = 5] = "Friday";
    DayOfWeek[DayOfWeek["Saturday"] = 6] = "Saturday";
})(DayOfWeek || (DayOfWeek = {}));
var DateTime = (function (_super) {
    __extends(DateTime, _super);
    function DateTime(year, month, day) {
        if (year === void 0) { year = 1; }
        if (month === void 0) { month = 1; }
        if (day === void 0) { day = 1; }
        _super.call(this);
        this.dt = new Date(year, month - 1, day);
        this.kind = DateTimeKind.Unspecified;
    }
    Object.defineProperty(DateTime.prototype, "Kind", {
        get: function () { return this.kind; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DateTime.prototype, "Year", {
        get: function () { return this.kind === DateTimeKind.Utc ? this.dt.getUTCFullYear() : this.dt.getFullYear(); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DateTime.prototype, "Month", {
        get: function () { return this.kind === DateTimeKind.Utc ? this.dt.getUTCMonth() + 1 : this.dt.getMonth() + 1; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DateTime.prototype, "Day", {
        get: function () { return this.kind === DateTimeKind.Utc ? this.dt.getUTCDate() : this.dt.getDate(); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DateTime.prototype, "DayOfWeek", {
        get: function () { return this.dt.getDay(); },
        enumerable: true,
        configurable: true
    });
    DateTime.prototype.ToString = function () {
        return this.kind === DateTimeKind.Utc ? this.dt.toUTCString() : this.dt.toString();
    };
    Object.defineProperty(DateTime, "UtcNow", {
        get: function () {
            var d = new DateTime();
            d.dt = new Date();
            d.kind = DateTimeKind.Utc;
            return d;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DateTime, "Now", {
        get: function () {
            var d = new DateTime();
            d.dt = new Date();
            d.kind = DateTimeKind.Local;
            return d;
        },
        enumerable: true,
        configurable: true
    });
    DateTime.op_Subtraction = function (x, y) {
        return TimeSpan.FromSeconds((x.dt.getTime() - y.dt.getTime()) / 1000);
    };
    DateTime.op_GreaterThanOrEqual = function (x, y) {
        return x.dt >= y.dt;
    };
    return DateTime;
}(NObject));
var TimeSpan = (function (_super) {
    __extends(TimeSpan, _super);
    function TimeSpan(ticks) {
        _super.call(this);
        this.ticks = ticks;
    }
    Object.defineProperty(TimeSpan.prototype, "TotalDays", {
        get: function () {
            throw new NotImplementedException();
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TimeSpan.prototype, "Days", {
        get: function () {
            throw new NotImplementedException();
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TimeSpan.prototype, "Hours", {
        get: function () {
            throw new NotImplementedException();
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TimeSpan.prototype, "Minutes", {
        get: function () {
            throw new NotImplementedException();
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(TimeSpan.prototype, "Seconds", {
        get: function () {
            throw new NotImplementedException();
        },
        enumerable: true,
        configurable: true
    });
    TimeSpan.FromSeconds = function (seconds) {
        return new TimeSpan(seconds * 100e9);
    };
    TimeSpan.FromDays = function (days) {
        var hours = days * 24;
        var minutes = 60 * hours;
        return TimeSpan.FromSeconds(60 * minutes);
    };
    TimeSpan.op_GreaterThanOrEqual = function (x, y) {
        return x.ticks >= y.ticks;
    };
    return TimeSpan;
}(NObject));
var NConsole = (function (_super) {
    __extends(NConsole, _super);
    function NConsole() {
        _super.apply(this, arguments);
    }
    NConsole.WriteLine = function (lineOrFormat, arg0) {
        throw new NotImplementedException();
    };
    return NConsole;
}(NObject));
var ArgumentException = (function (_super) {
    __extends(ArgumentException, _super);
    function ArgumentException(nameOrMessage, name) {
        _super.call(this);
    }
    return ArgumentException;
}(Exception));
var ArgumentNullException = (function (_super) {
    __extends(ArgumentNullException, _super);
    function ArgumentNullException(name) {
        _super.call(this, name);
    }
    return ArgumentNullException;
}(ArgumentException));
var ArgumentOutOfRangeException = (function (_super) {
    __extends(ArgumentOutOfRangeException, _super);
    function ArgumentOutOfRangeException(name) {
        _super.call(this, name);
    }
    return ArgumentOutOfRangeException;
}(ArgumentException));
var EventArgs = (function (_super) {
    __extends(EventArgs, _super);
    function EventArgs() {
        _super.apply(this, arguments);
    }
    return EventArgs;
}(NObject));
var EventHandler = (function (_super) {
    __extends(EventHandler, _super);
    function EventHandler() {
        _super.apply(this, arguments);
    }
    EventHandler.prototype.Invoke = function (sender, e) {
    };
    return EventHandler;
}(NObject));
var InvalidOperationException = (function (_super) {
    __extends(InvalidOperationException, _super);
    function InvalidOperationException() {
        _super.apply(this, arguments);
    }
    return InvalidOperationException;
}(Exception));
var Environment = (function () {
    function Environment() {
    }
    Environment.NewLine = "\n";
    return Environment;
}());
var Convert = (function (_super) {
    __extends(Convert, _super);
    function Convert() {
        _super.apply(this, arguments);
    }
    Convert.ToUInt16 = function (str) {
        var value = Number(str);
        if (value < 0)
            value = 0;
        if (value >= 0xFFFF)
            value = 0xFFFF;
        return value;
    };
    Convert.ToUInt32 = function (str) {
        var value = Number(str);
        if (value < 0)
            value = 0;
        if (value >= 0xFFFFFFFF)
            value = 0xFFFFFFFF;
        return value;
    };
    Convert.ToString = function (num, radixOrProvider) {
        throw new NotImplementedException();
    };
    return Convert;
}(NObject));
var NumberFormatInfo = (function (_super) {
    __extends(NumberFormatInfo, _super);
    function NumberFormatInfo() {
        _super.apply(this, arguments);
        this.NumberDecimalSeparator = ".";
        this.NumberGroupSeparator = ",";
    }
    return NumberFormatInfo;
}(NObject));
var NumberStyles;
(function (NumberStyles) {
    NumberStyles[NumberStyles["HexNumber"] = 0] = "HexNumber";
})(NumberStyles || (NumberStyles = {}));
var Encoding = (function (_super) {
    __extends(Encoding, _super);
    function Encoding() {
        _super.apply(this, arguments);
    }
    Encoding.UTF8 = new Encoding();
    return Encoding;
}(NObject));
var CultureInfo = (function (_super) {
    __extends(CultureInfo, _super);
    function CultureInfo(name) {
        _super.call(this);
        this.Name = "Invariant";
        this.nfi = new NumberFormatInfo();
    }
    CultureInfo.prototype.GetFormat = function (type) {
        if (type.Name === "NumberFormatInfo") {
            return this.nfi;
        }
        return null;
    };
    CultureInfo.InvariantCulture = new CultureInfo("Invariant");
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    return CultureInfo;
}(NObject));
var NotImplementedException = (function (_super) {
    __extends(NotImplementedException, _super);
    function NotImplementedException(message) {
        if (message === void 0) { message = "Not implemented"; }
        _super.call(this, message);
    }
    return NotImplementedException;
}(Exception));
var NotSupportedException = (function (_super) {
    __extends(NotSupportedException, _super);
    function NotSupportedException(message) {
        if (message === void 0) { message = "Not supported"; }
        _super.call(this, message);
    }
    return NotSupportedException;
}(Exception));
var OverflowException = (function (_super) {
    __extends(OverflowException, _super);
    function OverflowException() {
        _super.call(this, "Overflow");
    }
    return OverflowException;
}(Exception));
var List = (function (_super) {
    __extends(List, _super);
    function List(itemsOrCapacity) {
        _super.call(this);
        this.array = new Array(); // Public to help the enumerator
        if (arguments.length == 1 && itemsOrCapacity.constructor !== Number) {
            this.AddRange(itemsOrCapacity);
        }
    }
    List.prototype.Add = function (item) {
        this.array.push(item);
    };
    List.prototype.AddRange = function (items) {
        var e = items.GetEnumerator();
        while (e.MoveNext()) {
            this.Add(e.Current);
        }
    };
    Object.defineProperty(List.prototype, "Count", {
        get: function () {
            return this.array.length;
        },
        enumerable: true,
        configurable: true
    });
    List.prototype.get_Item = function (index) {
        return this.array[index];
    };
    List.prototype.set_Item = function (index, value) {
        this.array[index] = value;
    };
    List.prototype.GetEnumerator = function () {
        return new List_Enumerator(this);
    };
    List.prototype.RemoveAt = function (index) {
        this.array.splice(index, 1);
    };
    List.prototype.RemoveRange = function (index, count) {
        throw new NotImplementedException();
    };
    List.prototype.Insert = function (index, item) {
        this.array.splice(index, 0, item);
    };
    List.prototype.Clear = function () {
        this.array = new Array();
    };
    List.prototype.ToArray = function () {
        return this.array.slice(0);
    };
    List.prototype.RemoveAll = function (predicate) {
        var newArray = new Array();
        for (var i = 0; i < this.array.length; i++) {
            if (!predicate(this.array[i]))
                newArray.push(this.array[i]);
        }
        this.array = newArray;
    };
    List.prototype.Reverse = function () {
        throw new NotImplementedException();
    };
    List.prototype.IndexOf = function (item) {
        return this.array.indexOf(item);
    };
    return List;
}(NObject));
var Array_Enumerator = (function (_super) {
    __extends(Array_Enumerator, _super);
    function Array_Enumerator(array) {
        _super.call(this);
        this.index = -1;
        this.array = array;
    }
    Array_Enumerator.prototype.MoveNext = function () {
        this.index++;
        return this.index < this.array.length;
    };
    Object.defineProperty(Array_Enumerator.prototype, "Current", {
        get: function () {
            return this.array[this.index];
        },
        enumerable: true,
        configurable: true
    });
    Array_Enumerator.prototype.Dispose = function () {
    };
    return Array_Enumerator;
}(NObject));
var Array_Enumerable = (function (_super) {
    __extends(Array_Enumerable, _super);
    function Array_Enumerable(array) {
        _super.call(this);
        this.array = array;
    }
    Array_Enumerable.prototype.GetEnumerator = function () {
        return new Array_Enumerator(this.array);
    };
    return Array_Enumerable;
}(NObject));
var List_Enumerator = (function (_super) {
    __extends(List_Enumerator, _super);
    function List_Enumerator(list) {
        _super.call(this, list.array);
    }
    return List_Enumerator;
}(Array_Enumerator));
var Stack = (function (_super) {
    __extends(Stack, _super);
    function Stack() {
        _super.apply(this, arguments);
    }
    Stack.prototype.Push = function (item) {
        this.Add(item);
    };
    Stack.prototype.Pop = function () {
        throw new NotImplementedException();
    };
    return Stack;
}(List));
var HashSet = (function (_super) {
    __extends(HashSet, _super);
    function HashSet() {
        _super.apply(this, arguments);
        this.store = {};
    }
    HashSet.prototype.Add = function (item) {
        throw new NotImplementedException();
    };
    HashSet.prototype.GetEnumerator = function () {
        throw new NotImplementedException();
    };
    HashSet.prototype.Contains = function (item) {
        throw new NotImplementedException();
    };
    Object.defineProperty(HashSet.prototype, "Count", {
        get: function () {
            throw new NotImplementedException();
        },
        enumerable: true,
        configurable: true
    });
    return HashSet;
}(NObject));
var HashSet_Enumerator = (function (_super) {
    __extends(HashSet_Enumerator, _super);
    function HashSet_Enumerator() {
        _super.apply(this, arguments);
    }
    HashSet_Enumerator.prototype.MoveNext = function () {
        throw new NotImplementedException();
    };
    Object.defineProperty(HashSet_Enumerator.prototype, "Current", {
        get: function () {
            throw new NotImplementedException();
        },
        enumerable: true,
        configurable: true
    });
    HashSet_Enumerator.prototype.Dispose = function () {
    };
    return HashSet_Enumerator;
}(NObject));
var KeyValuePair = (function (_super) {
    __extends(KeyValuePair, _super);
    function KeyValuePair(key, value) {
        _super.call(this);
        this.Key = key;
        this.Value = value;
    }
    return KeyValuePair;
}(NObject));
var Dictionary = (function (_super) {
    __extends(Dictionary, _super);
    function Dictionary(other) {
        _super.call(this);
        this.keys = {};
        this.values = {};
    }
    Dictionary.prototype.get_Item = function (key) {
        return this.values[this.GetKeyString(key)];
    };
    Dictionary.prototype.set_Item = function (key, value) {
        var ks = this.GetKeyString(key);
        if (!this.values.hasOwnProperty(ks)) {
            this.keys[ks] = key;
        }
        this.values[ks] = value;
    };
    Dictionary.prototype.Add = function (key, value) {
        var ks = this.GetKeyString(key);
        if (this.values.hasOwnProperty(ks)) {
            throw new InvalidOperationException();
        }
        else {
            this.keys[ks] = key;
            this.values[ks] = value;
        }
    };
    Dictionary.prototype.GetKeyString = function (key) {
        if (key === null)
            return "null";
        if (typeof key === "undefined")
            return "undefined";
        return key + "";
    };
    Dictionary.prototype.ContainsKey = function (key) {
        return this.values.hasOwnProperty(this.GetKeyString(key));
    };
    Dictionary.prototype.TryGetValue = function (key, pvalue) {
        var ks = this.GetKeyString(key);
        if (this.values.hasOwnProperty(ks)) {
            pvalue[0] = this.values[ks];
            return true;
        }
        else {
            pvalue[0] = null;
            return false;
        }
    };
    Dictionary.prototype.Remove = function (key) {
        var ks = this.GetKeyString(key);
        delete this.values[ks];
        delete this.keys[ks];
    };
    Dictionary.prototype.Clear = function () {
        this.values = {};
        this.keys = {};
    };
    Object.defineProperty(Dictionary.prototype, "Count", {
        get: function () {
            return Object.keys(this.values).length;
        },
        enumerable: true,
        configurable: true
    });
    Dictionary.prototype.GetEnumerator = function () {
        var kvs = new List();
        for (var ks in this.values) {
            kvs.Add(new KeyValuePair(this.keys[ks], this.values[ks]));
        }
        return new Dictionary_Enumerator(kvs);
    };
    Object.defineProperty(Dictionary.prototype, "Keys", {
        get: function () {
            var keys = new Dictionary_KeyCollection();
            for (var ks in this.keys) {
                keys.Add(this.keys[ks]);
            }
            return keys;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Dictionary.prototype, "Values", {
        get: function () {
            var vals = new Dictionary_ValueCollection();
            for (var ks in this.values) {
                vals.Add(this.values[ks]);
            }
            return vals;
        },
        enumerable: true,
        configurable: true
    });
    return Dictionary;
}(NObject));
var Dictionary_Enumerator = (function (_super) {
    __extends(Dictionary_Enumerator, _super);
    function Dictionary_Enumerator(list) {
        _super.call(this, list);
    }
    return Dictionary_Enumerator;
}(List_Enumerator));
var Dictionary_KeyCollection = (function (_super) {
    __extends(Dictionary_KeyCollection, _super);
    function Dictionary_KeyCollection() {
        _super.apply(this, arguments);
    }
    return Dictionary_KeyCollection;
}(List));
var Dictionary_KeyCollection_Enumerator = (function (_super) {
    __extends(Dictionary_KeyCollection_Enumerator, _super);
    function Dictionary_KeyCollection_Enumerator(list) {
        _super.call(this, list);
    }
    return Dictionary_KeyCollection_Enumerator;
}(List_Enumerator));
var Dictionary_ValueCollection = (function (_super) {
    __extends(Dictionary_ValueCollection, _super);
    function Dictionary_ValueCollection() {
        _super.apply(this, arguments);
    }
    return Dictionary_ValueCollection;
}(List));
var Dictionary_ValueCollection_Enumerator = (function (_super) {
    __extends(Dictionary_ValueCollection_Enumerator, _super);
    function Dictionary_ValueCollection_Enumerator(list) {
        _super.call(this, list);
    }
    return Dictionary_ValueCollection_Enumerator;
}(List_Enumerator));
var Regex = (function (_super) {
    __extends(Regex, _super);
    function Regex(pattern) {
        _super.call(this);
        this.re = new RegExp(pattern, "g");
    }
    Regex.prototype.Match = function (input) {
        var m = new Match();
        var r = this.re.exec(input);
        if (r) {
            var loc = r.index;
            for (var i = 0; i < r.length; ++i) {
                var text = "";
                if (typeof r[i] === "undefined") { }
                else if (r[i].constructor === String)
                    text = r[i];
                m.Groups.Add(new Group(text, loc));
                if (i !== 0)
                    loc += text.length;
            }
            m.Success = true;
        }
        return m;
    };
    Regex.prototype.Replace = function (input, repl) {
        return input.replace(this.re, repl);
    };
    Regex.prototype.IsMatch = function (input) {
        return this.re.test(input);
    };
    return Regex;
}(NObject));
var Match = (function (_super) {
    __extends(Match, _super);
    function Match() {
        _super.apply(this, arguments);
        this.Groups = new List();
        this.Success = false;
    }
    return Match;
}(NObject));
var Group = (function (_super) {
    __extends(Group, _super);
    function Group(value, index) {
        _super.call(this);
        this.Length = 0;
        this.Value = "";
        this.Index = 0;
        this.Value = value || "";
        this.Length = this.Value.length;
        this.Index = index;
    }
    return Group;
}(NObject));
var Stream = (function (_super) {
    __extends(Stream, _super);
    function Stream() {
        _super.apply(this, arguments);
    }
    return Stream;
}(NObject));
var MemoryStream = (function (_super) {
    __extends(MemoryStream, _super);
    function MemoryStream() {
        _super.apply(this, arguments);
    }
    MemoryStream.prototype.ToArray = function () {
        throw new NotImplementedException();
    };
    return MemoryStream;
}(Stream));
var TextWriter = (function (_super) {
    __extends(TextWriter, _super);
    function TextWriter() {
        _super.apply(this, arguments);
    }
    TextWriter.prototype.Write = function (text) {
        throw new NotSupportedException();
    };
    TextWriter.prototype.WriteLine = function (text) {
        this.Write(text + Environment.NewLine);
    };
    TextWriter.prototype.Flush = function () {
        throw new NotSupportedException();
    };
    TextWriter.prototype.Dispose = function () {
    };
    return TextWriter;
}(NObject));
var StreamWriter = (function (_super) {
    __extends(StreamWriter, _super);
    function StreamWriter(streamOrPath, encoding) {
        _super.call(this);
    }
    return StreamWriter;
}(TextWriter));
var BinaryWriter = (function (_super) {
    __extends(BinaryWriter, _super);
    function BinaryWriter(baseStream, encoding) {
        _super.call(this);
    }
    BinaryWriter.prototype.Write = function (n) {
        throw new NotImplementedException();
    };
    BinaryWriter.prototype.Flush = function () {
        throw new NotImplementedException();
    };
    return BinaryWriter;
}(NObject));
var StringBuilder = (function (_super) {
    __extends(StringBuilder, _super);
    function StringBuilder() {
        _super.apply(this, arguments);
        this.parts = new Array();
    }
    StringBuilder.prototype.Append = function (textOrChar) {
        var text = (textOrChar.constructor == Number) ? String.fromCharCode(textOrChar) : textOrChar;
        this.parts.push(text);
    };
    StringBuilder.prototype.AppendLine = function (text) {
        if (text === void 0) { text = null; }
        if (text !== null) {
            this.parts.push(text);
        }
        this.parts.push(Environment.NewLine);
    };
    StringBuilder.prototype.AppendFormat = function (textOrFormat, arg0, arg1, arg2) {
        throw new NotImplementedException();
    };
    StringBuilder.prototype.ToString = function () {
        return this.parts.join("");
    };
    Object.defineProperty(StringBuilder.prototype, "Length", {
        get: function () {
            var len = 0;
            for (var i = 0; i < this.parts.length; i++) {
                len += this.parts[i].length;
            }
            return len;
        },
        enumerable: true,
        configurable: true
    });
    StringBuilder.prototype.get_Item = function (index) {
        var o = 0;
        for (var i = 0; i < this.parts.length; ++i) {
            var p = this.parts[i];
            if (index < o + p.length) {
                return p.charCodeAt(index - o);
            }
            o += p.length;
        }
        return 0;
    };
    return StringBuilder;
}(NObject));
var TextReader = (function (_super) {
    __extends(TextReader, _super);
    function TextReader() {
        _super.apply(this, arguments);
    }
    TextReader.prototype.ReadLine = function () {
        throw new NotSupportedException();
    };
    TextReader.prototype.ReadToEnd = function () {
        throw new NotSupportedException();
    };
    TextReader.prototype.Dispose = function () {
    };
    return TextReader;
}(NObject));
var StringReader = (function (_super) {
    __extends(StringReader, _super);
    function StringReader(str) {
        _super.call(this);
        this.str = str;
        this.pos = 0;
    }
    StringReader.prototype.ReadLine = function () {
        var p = this.pos;
        if (p >= this.str.length)
            return null;
        var end = p;
        while (end < this.str.length && this.str.charCodeAt(end) !== 10) {
            end++;
        }
        var tend = end;
        if (tend > p && this.str.charCodeAt(tend - 1) == 13) {
            tend--;
        }
        var r = this.str.substr(p, tend - p);
        this.pos = end + 1;
        return r;
    };
    return StringReader;
}(TextReader));
var StringWriter = (function (_super) {
    __extends(StringWriter, _super);
    function StringWriter() {
        _super.apply(this, arguments);
    }
    return StringWriter;
}(TextWriter));
//
// System.Linq
//
var Enumerable = (function (_super) {
    __extends(Enumerable, _super);
    function Enumerable() {
        _super.apply(this, arguments);
    }
    Enumerable.ToArray = function (e) {
        throw new NotImplementedException();
    };
    Enumerable.ToList = function (e) {
        return new List(e);
    };
    Enumerable.Empty = function () {
        return new List();
    };
    Enumerable.Range = function (start, count) {
        var end = start + count;
        var r = new List();
        for (var i = start; i < end; i++) {
            r.Add(i);
        }
        return r;
    };
    Enumerable.Select = function (e, selector) {
        var r = new List();
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            r.Add(selector(i.Current));
        }
        return r;
    };
    Enumerable.SelectMany = function (e, selector, comb) {
        throw new NotImplementedException();
    };
    Enumerable.Where = function (e, p) {
        var r = new List();
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            if (p(i.Current))
                r.Add(i.Current);
        }
        return r;
    };
    Enumerable.OrderBy = function (e, s) {
        var r = new List();
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            r.Add(i.Current);
        }
        r.array.sort(function (x, y) {
            var sx = s(x);
            var sy = s(y);
            if (sx === sy)
                return 0;
            if (sx < sy)
                return -1;
            return 1;
        });
        return r;
    };
    Enumerable.OrderByDescending = function (e, s) {
        var r = new List();
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            r.Add(i.Current);
        }
        r.array.sort(function (x, y) {
            var sx = s(x);
            var sy = s(y);
            if (sx === sy)
                return 0;
            if (sx < sy)
                return 1;
            return -1;
        });
        return r;
    };
    Enumerable.ThenBy = function (e, s) {
        return Enumerable.OrderBy(e, s);
    };
    Enumerable.Concat = function (x, y) {
        var r = new List(x);
        r.AddRange(y);
        return r;
    };
    Enumerable.Take = function (e, count) {
        var r = new List();
        var i = e.GetEnumerator();
        while (r.Count < count && i.MoveNext()) {
            r.Add(i.Current);
        }
        return r;
    };
    Enumerable.Skip = function (e, count) {
        var r = new List();
        var i = e.GetEnumerator();
        var j = 0;
        while (i.MoveNext()) {
            if (j >= count)
                r.Add(i.Current);
            j++;
        }
        return r;
    };
    Enumerable.Distinct = function (e) {
        var d = new Dictionary();
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            d.set_Item(i.Current, null);
        }
        return d.Keys;
    };
    Enumerable.Cast = function (e) {
        return e;
    };
    Enumerable.OfType = function (e) {
        // Doesn't work. Stupid type erasure.
        // var i = e.GetEnumerator();
        // var r = new List<U>();
        // while (i.MoveNext()) {
        // 	if (i.Current instanceof U) r.Add (i.Current);
        // }
        // return r;
        throw new NotImplementedException();
    };
    Enumerable.Contains = function (e, val) {
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            if (i.Current === val)
                return true;
        }
        return false;
    };
    Enumerable.FirstOrDefault = function (e, p) {
        if (p === void 0) { p = null; }
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            if (p === null || p(i.Current))
                return i.Current;
        }
        return null;
    };
    Enumerable.LastOrDefault = function (e, p) {
        if (p === void 0) { p = null; }
        var i = e.GetEnumerator();
        var last = null;
        while (i.MoveNext()) {
            if (p === null || p(i.Current))
                last = i.Current;
        }
        return last;
    };
    Enumerable.Last = function (e, p) {
        if (p === void 0) { p = null; }
        var i = e.GetEnumerator();
        var last = null;
        var gotLast = false;
        while (i.MoveNext()) {
            if (p === null || p(i.Current)) {
                last = i.Current;
                gotLast = true;
            }
        }
        if (gotLast)
            return last;
        throw new Exception("Not found");
    };
    Enumerable.First = function (e, p) {
        if (p === void 0) { p = null; }
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            if (p === null || p(i.Current))
                return i.Current;
        }
        throw new Exception("Not found");
    };
    Enumerable.Any = function (e, p) {
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            if (p(i.Current))
                return true;
        }
        return false;
    };
    Enumerable.All = function (e, p) {
        var i = e.GetEnumerator();
        while (i.MoveNext()) {
            if (!p(i.Current))
                return false;
        }
        return true;
    };
    Enumerable.Count = function (e) {
        throw new NotImplementedException();
    };
    Enumerable.Sum = function (e, s) {
        throw new NotImplementedException();
    };
    Enumerable.Max = function (e, s) {
        throw new NotImplementedException();
    };
    Enumerable.Min = function (e, s) {
        throw new NotImplementedException();
    };
    Enumerable.ToDictionary = function (e, k, v) {
        throw new NotImplementedException();
    };
    return Enumerable;
}(NObject));
var PropertyChangedEventArgs = (function (_super) {
    __extends(PropertyChangedEventArgs, _super);
    function PropertyChangedEventArgs(name) {
        _super.call(this);
    }
    return PropertyChangedEventArgs;
}(EventArgs));
var Debug = (function (_super) {
    __extends(Debug, _super);
    function Debug() {
        _super.apply(this, arguments);
    }
    Debug.WriteLine = function (text) {
        console.log(text);
    };
    return Debug;
}(NObject));
var Thread = (function (_super) {
    __extends(Thread, _super);
    function Thread() {
        _super.call(this);
        this.ManagedThreadId = Thread.nextId++;
    }
    Thread.nextId = 1;
    Thread.CurrentThread = new Thread();
    return Thread;
}(NObject));
var ThreadPool = (function (_super) {
    __extends(ThreadPool, _super);
    function ThreadPool() {
        _super.apply(this, arguments);
    }
    ThreadPool.QueueUserWorkItem = function (workItem) {
        throw new NotImplementedException();
    };
    return ThreadPool;
}(NObject));
var Monitor = (function (_super) {
    __extends(Monitor, _super);
    function Monitor() {
        _super.apply(this, arguments);
    }
    Monitor.Enter = function (lock) {
    };
    Monitor.Exit = function (lock) {
    };
    return Monitor;
}(NObject));
var Interlocked = (function (_super) {
    __extends(Interlocked, _super);
    function Interlocked() {
        _super.apply(this, arguments);
    }
    Interlocked.CompareExchange = function (location1, value, comparand) {
        var v = location1[0];
        if (v === comparand)
            location1[0] = value;
        return v;
    };
    return Interlocked;
}(NObject));
var WebClient = (function (_super) {
    __extends(WebClient, _super);
    function WebClient() {
        _super.apply(this, arguments);
    }
    WebClient.prototype.DownloadString = function (url) {
        throw new NotImplementedException();
    };
    return WebClient;
}(NObject));
var Random = (function (_super) {
    __extends(Random, _super);
    function Random(seed) {
        _super.call(this);
        this.seed = seed;
    }
    Random.prototype.Next = function (min, max) {
        max = max || 0;
        min = min || 0;
        this.seed = (this.seed * 9301 + 49297) % 233280;
        var rnd = this.seed / 233280;
        return min + rnd * (max - min);
    };
    // http://indiegamr.com/generate-repeatable-random-numbers-in-js/
    Random.prototype.NextInt = function (min, max) {
        return Math.round(this.Next(min, max));
    };
    Random.prototype.NextDouble = function () {
        return this.Next(0, 1);
    };
    Random.prototype.pick = function (collection) {
        return collection[this.NextInt(0, collection.length - 1)];
    };
    return Random;
}(NObject));
//# sourceMappingURL=mscorlib.js.map