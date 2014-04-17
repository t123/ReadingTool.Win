<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" omit-xml-declaration="yes" />
  <xsl:template match="/root">
    <xsl:apply-templates select="content"/>
  </xsl:template>
  <xsl:template match="content">
    <table id="reading">
      <xsl:attribute name="data-webapi">
        <xsl:value-of select="/root/content/@webApi"/>
      </xsl:attribute>
      <xsl:attribute name="data-signalr">
        <xsl:value-of select="/root/content/@signalR"/>
      </xsl:attribute>
      <xsl:attribute name="data-languageid">
        <xsl:value-of select="/root/content/@l1Id"/>
      </xsl:attribute>
      <xsl:attribute name="data-itemid">
        <xsl:value-of select="/root/content/@itemId"/>
      </xsl:attribute>
      <xsl:attribute name="data-itemtype">
        <xsl:value-of select="/root/content/@itemType"/>
      </xsl:attribute>
      <xsl:attribute name="data-l1code">
        <xsl:value-of select="/root/content/@l1Code"/>
      </xsl:attribute>
      <xsl:attribute name="data-l2code">
        <xsl:value-of select="/root/content/@l2Code"/>
      </xsl:attribute>
      
      <tr width="100%">
        <td id="l1Main">
          <xsl:comment>output</xsl:comment>
        </td>
      </tr>
      <tr width="100%">
        <td id="l2Main">
          <xsl:comment>output</xsl:comment>
        </td>
      </tr>
    </table>
    <div id="parsed" style="display:none">
      <table>
        <xsl:apply-templates select="join"/>
      </table>
    </div>
  </xsl:template>
  <xsl:template match="join">
    <tr>
      <xsl:apply-templates select="paragraph" />
      <xsl:choose>
        <xsl:when test="/root/content/@isParallel='true'">
          <td>
            <xsl:attribute name="id">l2_<xsl:value-of select="@line"/></xsl:attribute>
            <xsl:value-of select="translation"/>
          </td>
        </xsl:when>
      </xsl:choose>
    </tr>
  </xsl:template>
  <xsl:template match="paragraph">
    <td>
      <xsl:attribute name="id">l1_<xsl:value-of select="../@line"/></xsl:attribute>
      <xsl:apply-templates select="sentence"/>
    </td>
  </xsl:template>
  <xsl:template match="sentence">
    <p class="__sentence">
      <xsl:apply-templates select="term"/>
    </p>
  </xsl:template>
  <xsl:template match="term">
    <xsl:choose>
      <xsl:when test="@isTerm='true'">
        <span>
          <xsl:attribute name="class">
            <xsl:text>__term</xsl:text>
            <xsl:text> __</xsl:text>
            <xsl:value-of select="@state"/>
            <xsl:text> __</xsl:text>
            <xsl:value-of select="@phraseClass"/>
            <xsl:if test="@commonness">
              <xsl:text> __</xsl:text>
              <xsl:value-of select="@commonness"/>
            </xsl:if>
          </xsl:attribute>
          <xsl:attribute name="data-frequency">
            <xsl:value-of select="@frequency"/>
          </xsl:attribute>
          <xsl:attribute name="data-occurrences">
            <xsl:value-of select="@occurrences"/>
          </xsl:attribute>
          <xsl:value-of select="."/>
        </span>
      </xsl:when>
      <xsl:otherwise>
        <span class="__punctuation">
          <xsl:value-of select="."/>
        </span>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>