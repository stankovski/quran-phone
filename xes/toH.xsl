<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="text"/>
<xsl:strip-space elements="*"/>
<xsl:template match="/">
<xsl:apply-templates select="*" />
</xsl:template>

<xsl:template match="comment">
<xsl:value-of select="."/>
</xsl:template>

<xsl:template match="package">
<xsl:apply-templates/>
</xsl:template>

<xsl:template match="importList">
<xsl:apply-templates select="*"/>
</xsl:template>

<xsl:template match="import">
#import <xsl:value-of select="@name"/>;</xsl:template>

<xsl:template match="classDef">
<xsl:text>
</xsl:text>
<xsl:if test="@public">public:<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@private">private:<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@protected">protected:<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@static">static<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@abstract">abstract<xsl:text> </xsl:text></xsl:if>class <xsl:value-of select="@name"/>
<xsl:apply-templates select="nameList"/>{
<xsl:apply-templates select="*[not(self::nameList)]"/>

}
</xsl:template>

<xsl:template match="method">
<xsl:text>
</xsl:text>
<xsl:apply-templates select="comment[position() = 1]"/>
<xsl:if test="@public">public:<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@private">private:<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@protected">protected:<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@static">static<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@abstract">abstract<xsl:text> </xsl:text></xsl:if>
<xsl:if test="@type"><xsl:value-of select="@type"/><xsl:text> </xsl:text></xsl:if>
<xsl:value-of select="@name"/><xsl:apply-templates select="comment"/>
<xsl:value-of select="@name"/><xsl:apply-templates select="parameterList"/><xsl:text>;</xsl:text>
</xsl:template>

<xsl:template match="parameterList">
<xsl:text/>(<xsl:apply-templates select="*"/>)<xsl:text/>
</xsl:template>

<xsl:template match="parameter">
<xsl:value-of select="@type"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>
<xsl:if test="position() != last()">,</xsl:if>
</xsl:template>

<xsl:template match="nameList">
<xsl:text> </xsl:text><xsl:value-of select="@elementName"/><xsl:text> </xsl:text><xsl:value-of select="@type"/>
</xsl:template>

<xsl:template match="variable">
<xsl:apply-templates select="*"/>
<xsl:value-of select="@type"/><xsl:text> </xsl:text><xsl:value-of select="@name"/><xsl:apply-templates select="*"/>
<xsl:if test="parent::classDef">;
</xsl:if>
<xsl:if test="parent::block">;
</xsl:if>
</xsl:template>

<xsl:template match="block">
<xsl:text/>{
<xsl:apply-templates select="*"/>
}
</xsl:template>

<xsl:template match="loop">
<xsl:value-of select="@code"/><xsl:text/>
<xsl:apply-templates select="*"/>
</xsl:template>

<xsl:template match="return">
return<xsl:apply-templates select="*"/>;
</xsl:template>

<xsl:template match="booleanExpression">
<xsl:text/>(<xsl:apply-templates select="*"/>)<xsl:text/>
</xsl:template>

<xsl:template match="if">
if<xsl:apply-templates select="*"/>
</xsl:template>

<xsl:template match="binaryOp">
<xsl:apply-templates select="*[1]"/>
<xsl:value-of select="@operator"/>
<xsl:apply-templates select="*[2]"/>
</xsl:template>

<xsl:template match="unaryOp">
<xsl:value-of select="@type"/>
<xsl:apply-templates select="*"/><xsl:if test="parent::block">;
</xsl:if>
</xsl:template>

<xsl:template match="array">
<xsl:apply-templates select="*[1]"/>[<xsl:apply-templates select="*[2]"/>]
</xsl:template>

<xsl:template match="assign">
<xsl:apply-templates select="*[1]"/>
<xsl:value-of select="@operator"/>
<xsl:apply-templates select="*[2]"/>
<xsl:if test="parent::block">;
</xsl:if>
</xsl:template>

<xsl:template match="break">
break<xsl:apply-templates select="*"/>;
</xsl:template>

<xsl:template match="call">

<xsl:value-of select="@name"/>(<xsl:for-each select="*"><xsl:if test="position() != 1">,</xsl:if>
<xsl:apply-templates select="."/></xsl:for-each>)<xsl:if test="parent::block">;
</xsl:if>
</xsl:template>

<xsl:template match="constant">
<xsl:if test="@stringConst"><xsl:value-of select="@stringConst"/><xsl:text> </xsl:text></xsl:if>
<xsl:if test="@intConst"><xsl:value-of select="@intConst"/><xsl:text> </xsl:text></xsl:if>
<xsl:if test="@doubleConst"><xsl:value-of select="@doubleConst"/><xsl:text> </xsl:text></xsl:if>
<xsl:if test="@boolConst"><xsl:value-of select="@boolConst"/><xsl:text> </xsl:text></xsl:if>
<xsl:apply-templates select="*"/>
</xsl:template>

<xsl:template match="switch">
switch<xsl:apply-templates select="*"/>
</xsl:template>

<xsl:template match="try">
try {<xsl:apply-templates select="*"/>
} catch (Exception e) {
}
</xsl:template>

</xsl:stylesheet>
