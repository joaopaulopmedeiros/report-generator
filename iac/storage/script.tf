resource "aws_s3_bucket" "example" {
  bucket = "dotnet-report-generator"

  tags = {
    Name        = "dotnet-report-generator"
    Environment = "STG"
  }
}