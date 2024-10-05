﻿using System.IO;

namespace Transcript.Models
{
    public class Transcript_JSON
    {
        public string? DeptId { get; set; }
        public string? StudentId { get; set; }
        public int Secno { get; set; }
        public int Grade { get; set; }
        public int Clacod { get; set; }
        public int syearEnd { get; set; }
        public int semEnd { get; set; }
        public bool Isrank { get; set; }
        public bool Isgrading { get; set; }
    }

    public class informStudent
    {
        public int Acad { get; set; }
        public string? AcadName { get; set; }
        public string? Stuno { get; set; }
        public string? College { get; set; }
        public string? Department { get; set; }
        public string? Grade { get; set; }
        public string? Clacod { get; set; }
        public string? Name { get; set; }
        public string? Issued { get; set; }
        public int DataYear { get; set; }
        public int DataSemester { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Average { get; set; }
        public decimal ConductMark { get; set; }
    }

    public class Student
    {
        public int Acad { get; set; }
        public string? Name { get; set; }
        public string? Birth { get; set; }
        public string? Enrolled { get; set; }
        public string? DegreeConferred { get; set; }
        public string? DateConferred { get; set; }
        public string? Department { get; set; }
        public string? College { get; set; }
        public string? Issued { get; set; }
        public decimal Score { get; set; }
        public decimal Credits { get; set; }
        public decimal Rank { get; set; }
        public decimal Class { get; set; }
        public string? Percent
        {
            get
            {
                return $"{Rank}/{Class} ({(Rank / (Class == 0 ? 1 : Class)).ToString("0.00%")})";
            }
        }
        public decimal GPA { get; set; }
        public bool IsRank { get; set; }
        public decimal Thesis { get; set; }
        public int TR { get; set; }
    }

    public class Courses
    {
        public string? Course { get; set; }
        public int Capacity { get; set; }
        public decimal Credit { get; set; }
        public string? Grade { get; set; }
        public int DataYear { get; set; }
        public int DataSemester { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Average { get; set; }
        public decimal ConductMark { get; set; }
    }

    public class informCourses
    {
        public string? Course { get; set; }
        public string? Type { get; set; }
        public int Capacity { get; set; }
        public decimal Credit { get; set; }
        public string? Grade { get; set; }
        public bool Pass { get; set; }
    }

    public class Sign
    {
        public byte[] SignReg { get; set; }
        public byte[] SignDean { get; set; }
        public byte[] Seal { get; set; }
        public Sign(string path)
        {
            SignReg = File.ReadAllBytes(Path.Combine(path, "signreg.png"));
            SignDean = File.ReadAllBytes(Path.Combine(path, "signdean.png"));
            Seal = File.ReadAllBytes(Path.Combine(path, "seal.png"));
        }
    }
}