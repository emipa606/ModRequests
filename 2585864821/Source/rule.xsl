<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">
<Patch>	
    <xsl:for-each select="stat/s">
        <Operation Class="PatchOperationSequence">
            <operations>
                <li Class="PatchOperationConditional">
                    <xpath>Defs/StatDef[defName = "<xsl:value-of select="name"/>"]/parts</xpath>
                    <success>Always</success>
                    <nomatch Class="PatchOperationAdd">
                        <xpath>Defs/StatDef[defName="<xsl:value-of select="name"/>"]</xpath>
                        <value><parts/></value>
                    </nomatch>
                </li>
                <li Class="PatchOperationConditional">
                    <xpath>Defs/StatDef[defName = "<xsl:value-of select="name"/>"]/parts/li[@Class="StatPart_Quality"]</xpath>
                    <nomatch Class="PatchOperationAdd">
                        <xpath>Defs/StatDef[defName = "<xsl:value-of select="name"/>"]/parts</xpath>
                        <value>
                            <li Class="StatPart_Quality">
                                <xsl:call-template name="Factor"/>
                            </li>
                        </value>
                    </nomatch>
                </li>
            </operations>
        </Operation>
        
    </xsl:for-each>
</Patch>

</xsl:template>

<xsl:template name="Factor">
    <xsl:choose>
    <xsl:when test="factor = 'DefaultFactor'">
        <factorAwful>0.4</factorAwful>
        <factorPoor>0.75</factorPoor>
        <factorNormal>1</factorNormal>
        <factorGood>1.15</factorGood>
        <factorExcellent>1.25</factorExcellent>
        <factorMasterwork>1.3</factorMasterwork>
        <factorLegendary>1.35</factorLegendary>
    </xsl:when>
    
    <xsl:when test="factor = 'InverseFactor'">
        <factorAwful>1.35</factorAwful>
        <factorPoor>1.1</factorPoor>
        <factorNormal>1</factorNormal>
        <factorGood>0.9</factorGood>
        <factorExcellent>0.85</factorExcellent>
        <factorMasterwork>0.8</factorMasterwork>
        <factorLegendary>0.75</factorLegendary>
    </xsl:when>

    <!--
    <xsl:when test="factor = 'NegativeFactor'">
        <factorAwful>0.2</factorAwful>
        <factorPoor>0.4</factorPoor>
        <factorNormal>0.6</factorNormal>
        <factorGood>0.8</factorGood>
        <factorExcellent>0.85</factorExcellent>
        <factorMasterwork>0.9</factorMasterwork>
        <factorLegendary>0.95</factorLegendary>
    </xsl:when>
    -->
    </xsl:choose>
</xsl:template>

</xsl:stylesheet>