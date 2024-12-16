using Markdown;

namespace MarkdownTests
{
    public class Tests
    {
        private void AssertMarkdownRender(string source, string expected)
        {
            Assert.That(MdProcessor.Render(source), Is.EqualTo(expected));
        }

        [TestCase("_���������� � ���� ������_", "<em>���������� � ���� ������</em>")]
        public void IsValidItalic(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }

        [TestCase("__���������� ����� ��������� �����__", "<strong>���������� ����� ��������� �����</strong>")]
        public void IsValidBold(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }

        [TestCase("\\_��� ���\\_", "_��� ���_")]
        [TestCase("����� ���\\���� �������������\\ \\������ ��������.\\", "����� ���\\���� �������������\\ \\������ ��������.\\")]
        public void IsValidEscape(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }

        [TestCase("__�������� ��������� _���������_ ����__", "<strong>�������� ��������� <em>���������</em> ����</strong>")]
        [TestCase("_���������� __�������__ ��_", "<em>���������� __�������__ ��</em>")]
        [TestCase("�������_12_3", "�������_12_3")]
        [TestCase("_���_���, � � ���_���_��, � � ���_��._", "<em>���</em>���, � � ���<em>���</em>��, � � ���<em>��.</em>")]
        [TestCase("__��������_", "__��������_")]
        [TestCase("���_ ��������_", "���_ ��������_")]
        [TestCase("��� _�������� _�� ���������_", "��� _�������� _�� ���������_")]
        [TestCase("__����������� _�������__ � ���������_", "__����������� _�������__ � ���������_")]
        [TestCase("____", "____")]
        public void IsValidNested(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }

        [TestCase("# ���������", "<h1>���������</h1>")]
        [TestCase("# ��������� __� _�������_ ���������__", "<h1>��������� <strong>� <em>�������</em> ���������</strong></h1>")]
        public void IsValidHeader(string source, string expected)
        {
            AssertMarkdownRender(source, expected);
        }
    }
}
