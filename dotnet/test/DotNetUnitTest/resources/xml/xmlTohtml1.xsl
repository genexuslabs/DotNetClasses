<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"> 
<xsl:template match='/'> 
<html>
<head><title>Generado con xsl</title></head>
<body>
 <xsl:apply-templates select='msbuild' />
</body>
</html>
</xsl:template>

<xsl:template match='msbuild'>
<TABLE>
<TR>
<TD>
    <xsl:apply-templates/>
</TD>
</TR>
</TABLE>
</xsl:template>

<xsl:template match='*'>
<xsl:variable name="StatusVar" select="Status"/>
<xsl:variable name="TextoComp">
  <SPAN style="FONT-WEIGHT: bold; FONT-SIZE: 14pt; COLOR: #000000; FONT-FAMILY: Tahoma;" Font="Tahoma,12,B">
     <xsl:value-of select="name()"/>
  </SPAN>
</xsl:variable>
<div style="margin-left: 1.5em;">
<TABLE border="0">
<TR>
    <xsl:choose>
      <xsl:when test="$StatusVar = 'False'">
        <TD  style="BACKGROUND-COLOR: #ffaeb0">
            <xsl:copy-of select="$TextoComp" />
        </TD>
      </xsl:when>
      <xsl:when test="$StatusVar = 'True'">
        <TD  style="BACKGROUND-COLOR: #b1feaf">
           <xsl:copy-of select="$TextoComp" />
        </TD>
      </xsl:when>
      <xsl:otherwise>
        <TD>
            <xsl:copy-of select="$TextoComp" />
        </TD>
      </xsl:otherwise>
   </xsl:choose>
</TR>
<TR>
<TD>
    <xsl:apply-templates/>
</TD>
</TR>
</TABLE>
</div>
</xsl:template>

<xsl:template match='Message|Error|Warning'>
<xsl:variable name="LevelVar" select="Level"/>
<TR>
  <xsl:choose>
      <xsl:when test="$LevelVar = '3'">
          <TD>
          <SPAN style="FONT-SIZE: 10pt; COLOR: #000000; FONT-FAMILY: Tahoma;" Font="Tahoma,12,B">
             <xsl:value-of select='Info' />
          </SPAN>
          </TD>
      </xsl:when>
      <xsl:when test="$LevelVar = '2'">
          <TD  style="BACKGROUND-COLOR: #fbffa6">
          <SPAN style="FONT-SIZE: 10pt; COLOR: #000000; FONT-FAMILY: Tahoma;" Font="Tahoma,12,B">
             <xsl:value-of select='Info' />
          </SPAN>
          </TD>
      </xsl:when>
      <xsl:when test="$LevelVar = '1'">
          <TD  style="BACKGROUND-COLOR: #ffaeb0">
          <SPAN style="FONT-SIZE: 10pt; COLOR: #000000; FONT-FAMILY: Tahoma;" Font="Tahoma,12,B">
             <xsl:value-of select='Info' />
          </SPAN>
          </TD>
      </xsl:when>
  </xsl:choose>
</TR>
</xsl:template>

<xsl:template match='Status'>
</xsl:template>

</xsl:stylesheet>